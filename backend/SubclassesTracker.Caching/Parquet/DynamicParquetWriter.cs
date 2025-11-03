using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace SubclassesTracker.Caching.Parquet
{
    public interface IDynamicParquetWriter
    {
        /// <summary>
        /// Writes a collection of dynamic rows to a Parquet file.
        /// </summary>
        /// <param name="rows">Dictionary representing flatten rows to write</param>
        /// <param name="outputPath">Path to output Parquet file</param>
        /// <param name="compression">Compression method to use</param>
        /// <param name="ct">Token to cancel the operation</param>
        Task WriteAsync(
            IEnumerable<Dictionary<string, object?>> rows,
            string outputPath,
            CompressionMethod compression = CompressionMethod.Zstd,
            CancellationToken ct = default);
    }
    /// <summary>
    /// Data writer for dynamic Parquet files.
    /// </summary>
    public class DynamicParquetWriter(
        ILogger<DynamicParquetWriter> logger,
        IOptions<CachingSettings> options) : IDynamicParquetWriter
    {
        private readonly CachingSettings cachingSettings = options.Value;

        public async Task WriteAsync(
            IEnumerable<Dictionary<string, object?>> rows,
            string outputPath,
            CompressionMethod compression = CompressionMethod.Zstd,
            CancellationToken ct = default)
        {
            var rowList = rows.ToList();
            if (rowList.Count == 0) return;

            var allKeys = rowList.SelectMany(r => r.Keys).Distinct().ToList();
            var schemaFields = new List<DataField>();

            // --- Build schema and columns ---
            foreach (var key in allKeys)
            {
                // Infer column type from first non-null value
                object? firstVal = rowList.Select(r => r.TryGetValue(key, out var v) ? v : null).FirstOrDefault(v => v != null);
                Type type = firstVal?.GetType() ?? typeof(string); // fallback

                // Determine DataField type
                DataField field = type switch
                {
                    Type t when t == typeof(int) => new DataField<int>(key),
                    Type t when t == typeof(long) => new DataField<long>(key),
                    Type t when t == typeof(float) => new DataField<float>(key),
                    Type t when t == typeof(double) => new DataField<double>(key),
                    Type t when t == typeof(bool) => new DataField<bool>(key),
                    Type t when t == typeof(DateTime) => new DataField<DateTime>(key),
                    Type t when t == typeof(Guid) => new DataField<string>(key.ToString()),
                    Type t when t == typeof(string) => new DataField<string>(key),
                    _ => FallbackDataType(type, key)
                };

                schemaFields.Add(field);
            }

            // Prepare columns
            var schema = new ParquetSchema(schemaFields);
            var columns = new List<DataColumn>();
            foreach (var field in schema.GetDataFields())
            {
                Array data = Array.CreateInstance(field.ClrNullableIfHasNullsType, rowList.Count);
                for (int i = 0; i < rowList.Count; i++)
                    data.SetValue(rowList[i].TryGetValue(field.Name, out var v) ? v : null, i);

                // Add column
                columns.Add(new DataColumn(field, data));
            }

            // --- Write to Parquet file ---
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            using var fs = File.Create(outputPath);

            using var writer = await ParquetWriter.CreateAsync(schema, fs, cancellationToken: ct);
            writer.CompressionMethod = compression;

            using ParquetRowGroupWriter groupWriter = writer.CreateRowGroup();
            foreach (var col in columns)
            {
                await groupWriter.WriteColumnAsync(col, ct);
            }
        }

        private DataField<string> FallbackDataType(Type type, string key)
        {
            logger.LogWarning("Unsupported type {Type} for key {Key}, defaulting to string.", type, key);
            return new DataField<string>(key);
        }
    }

}
