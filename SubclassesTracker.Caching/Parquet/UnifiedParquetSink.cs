using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System.Runtime.CompilerServices;

namespace SubclassesTracker.Caching.Parquet
{
    public sealed class UnifiedParquetSink(string basePath, int rowGroupSize = 100_000)
    {
        private readonly string _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        private readonly int _rowGroupSize = rowGroupSize > 0 ? rowGroupSize : 100_000;

        public async Task WriteAsync<T>(
            IParquetDataset<T> dataset,
            IAsyncEnumerable<T> rows,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dataset);
            if (dataset.Mode == WriteModeEnum.ReplaceAll)
            {
                // ReplaceAll: write a single file atomically
                await WriteSingleFileAsync(dataset, rows, ct);
            }
            else
            {
                // Append: expect the stream to be for a single partition (period).
                // If you need multi-partition streaming, call this per partition.
                await WriteAppendSinglePartitionAsync(dataset, rows, ct);
            }
        }

        /// <summary>
        /// ReplaceAll path (dimensions)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataset"></param>
        /// <param name="rows"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task WriteSingleFileAsync<T>(
            IParquetDataset<T> dataset,
            IAsyncEnumerable<T> rows,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dataset.FixedFileName))
                throw new InvalidOperationException("FixedFileName must be set for ReplaceAll datasets.");

            var dir = Path.Combine(_basePath, dataset.Name);
            Directory.CreateDirectory(dir);

            var tmpPath = Path.Combine(dir, dataset.FixedFileName + ".tmp");
            var finalPath = Path.Combine(dir, dataset.FixedFileName);

            var schema = new ParquetSchema(dataset.Columns.Select(c => c.Field).ToArray());

            await using var fs = File.Create(tmpPath);
            using var writer = await ParquetWriter.CreateAsync(schema, fs, cancellationToken: ct);

            // Write in row groups (single file)
            await foreach (var batch in Batch(rows, _rowGroupSize, ct))
            {
                ct.ThrowIfCancellationRequested();
                using var rgw = writer.CreateRowGroup();

                foreach (var col in dataset.Columns)
                {
                    var arr = col.ProjectBatch(batch);
                    await rgw.WriteColumnAsync(new DataColumn(col.Field, arr), ct);
                }
            }

            fs.Close();
            File.Move(tmpPath, finalPath, overwrite: true);
        }

        /// <summary>
        /// Append path (facts)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataset"></param>
        /// <param name="rows"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task WriteAppendSinglePartitionAsync<T>(
            IParquetDataset<T> dataset,
            IAsyncEnumerable<T> rows,
            CancellationToken ct)
        {
            // Determine the partition from the first row and enforce single partition per call for simplicity.
            T? first = default;
            await foreach (var item in rows.WithCancellation(ct))
            {
                first = item;
                break;
            }
            if (first is null) return;

            var part = dataset.PartitionFolder(first);
            var remaining = YieldWithFirst(first, rows, ct);

            var dir = string.IsNullOrEmpty(part)
                ? Path.Combine(_basePath, dataset.Name)
                : Path.Combine(_basePath, dataset.Name, part);

            Directory.CreateDirectory(dir);

            var fileName = $"part-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.parquet";
            var tmpPath = Path.Combine(dir, fileName + ".tmp");
            var finalPath = Path.Combine(dir, fileName);

            var schema = new ParquetSchema(dataset.Columns.Select(c => c.Field).ToArray());

            await using var fs = File.Create(tmpPath);
            using var writer = await ParquetWriter.CreateAsync(schema, fs, cancellationToken: ct);

            await foreach (var batch in Batch(remaining, _rowGroupSize, ct))
            {
                // Validate same partition (defensive)
                if (batch.Any(x => dataset.PartitionFolder(x) != part))
                    throw new InvalidOperationException("Mixed partitions in a single append call. Split the stream per partition.");

                using var rgw = writer.CreateRowGroup();
                foreach (var col in dataset.Columns)
                {
                    var arr = col.ProjectBatch(batch);
                    await rgw.WriteColumnAsync(new DataColumn(col.Field, arr), ct);
                }
            }

            fs.Close();
            File.Move(tmpPath, finalPath, overwrite: false);
        }

        // --- helpers ---

        private static async IAsyncEnumerable<IReadOnlyList<T>> Batch<T>(
            IAsyncEnumerable<T> source,
            int batchSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var buf = new List<T>(batchSize);
            await foreach (var item in source.WithCancellation(ct))
            {
                buf.Add(item);
                if (buf.Count >= batchSize)
                {
                    yield return buf.ToArray();
                    buf.Clear();
                }
            }
            if (buf.Count > 0) yield return buf.ToArray();
        }

        private static async IAsyncEnumerable<T> YieldWithFirst<T>(
            T first,
            IAsyncEnumerable<T> rest,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return first;
            await foreach (var item in rest.WithCancellation(ct))
                yield return item;
        }
    }
}
