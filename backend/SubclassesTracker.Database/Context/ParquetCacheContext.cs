using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.CacheEntities;

namespace SubclassesTracker.Database.Context
{
    public class ParquetCacheContext(DbContextOptions<ParquetCacheContext> options) : DbContext(options)
    {
        public DbSet<FileEntry> FileEntries { get; set; } = null!;
        public DbSet<Partition> Partitions { get; set; } = null!;
        public DbSet<Dataset> Datasets { get; set; } = null!;
        public DbSet<RequestSnapshot> RequestSnapshots { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<FileEntry>()
                .HasOne(fe => fe.Partition)
                .WithMany(p => p.Files)
                .HasForeignKey(fe => fe.PartitionId);

            b.Entity<FileEntry>()
                .HasMany(fe => fe.RequestSnapshots)
                .WithMany(d => d.FileEntries)
                .UsingEntity("FileEntryRequestSnapshot");

            b.Entity<Partition>()
                .HasOne(p => p.Dataset)
                .WithMany(d => d.Partitions)
                .HasForeignKey(p => p.DatasetId);

            b.Entity<Dataset>()
                .HasMany(d => d.Files)
                .WithOne(fe => fe.Dataset)
                .HasForeignKey(fe => fe.DatasetId);

            b.Entity<RequestSnapshot>()
                .HasIndex(rs => new { rs.QueryName, rs.VarsHash })
                .IsUnique();

            base.OnModelCreating(b);
        }
    }
}
