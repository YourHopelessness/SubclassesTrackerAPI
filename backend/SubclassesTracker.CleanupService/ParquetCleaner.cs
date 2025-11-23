using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Context;

namespace SubclassesTracker.CleanupService;

public class ParquetCleaner(IServiceProvider sp, ILogger<ParquetCleaner> logger) : BackgroundService
{
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<ParquetCleaner> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupLoop(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parquet cleaner iteration failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    /// <summary>
    /// Main cleanup iteration: removes expired and invalid files,
    /// and deletes outdated (6+ months old) request snapshots.
    /// </summary>
    private async Task RunCleanupLoop(CancellationToken token)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ParquetCacheContext>();

        var now = DateTimeOffset.UtcNow;

        int filesDeleted = await CleanupFileEntries(db, now, token);
        int snapshotsDeleted = await CleanupOldSnapshots(db, now, token);

        await db.SaveChangesAsync(token);

        _logger.LogInformation(
            "Cleanup completed. Files removed: {files}, Snapshots removed: {snaps}",
            filesDeleted, snapshotsDeleted
        );
    }

    /// <summary>
    /// Cleans FileEntry records based on:
    ///  - TTL expiration;
    ///  - file missing on disk;
    ///  - no linked RequestSnapshots.
    /// </summary>
    private async Task<int> CleanupFileEntries(
        ParquetCacheContext db,
        DateTimeOffset now,
        CancellationToken token)
    {
        var files = await db.FileEntries
            .Include(x => x.Dataset)
            .Include(x => x.Partition)
            .Include(x => x.RequestSnapshots)
            .ToListAsync(token);

        int deleted = 0;

        foreach (var file in files)
        {
            bool shouldDelete = false;

            // TTL expiration
            var expiresAt = file.CachedAt + TimeSpan.FromSeconds(file.Ttl);
            if (expiresAt <= now)
            {
                shouldDelete = true;
                _logger.LogInformation("TTL expired for file {file}", file.FileName);
            }

            // File missing on disk
            var path = file.FullPath;
            if (!File.Exists(path))
            {
                shouldDelete = true;
                _logger.LogWarning("File missing: {path}", path);
            }

            // No linked snapshots
            if (file.RequestSnapshots == null || file.RequestSnapshots.Count == 0)
            {
                shouldDelete = true;
                _logger.LogInformation("Orphan file: {file}", file.FileName);
            }

            if (shouldDelete)
            {
                DeletePhysicalFile(path);

                db.FileEntries.Remove(file);
                deleted++;
            }
        }

        return deleted;
    }

    /// <summary>
    /// Deletes RequestSnapshot entries older than 6 months.
    /// Does NOT delete based on whether FileEntries exist.
    /// </summary>
    private async Task<int> CleanupOldSnapshots(
        ParquetCacheContext db,
        DateTimeOffset now,
        CancellationToken token)
    {
        var threshold = now.AddMonths(-6);

        var snapshots = await db.RequestSnapshots
            .Include(x => x.FileEntries)
            .Where(x => x.FileEntries.Count == 0)
            .Where(x => x.CreatedAt < threshold)
            .ToListAsync(token);

        if (snapshots.Count == 0)
            return 0;

        db.RequestSnapshots.RemoveRange(snapshots);

        _logger.LogInformation(
            "Removed empty {count} RequestSnapshots older than 6 months",
            snapshots.Count
        );

        return snapshots.Count;
    }

    /// <summary>
    /// Attempts to remove a physical file from disk with error logging.
    /// </summary>
    private void DeletePhysicalFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger.LogInformation("Deleted file: {path}", path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {path}", path);
        }
    }
}
