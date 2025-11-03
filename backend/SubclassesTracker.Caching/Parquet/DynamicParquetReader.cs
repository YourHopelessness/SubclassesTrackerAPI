using Microsoft.Extensions.Options;
using Parquet;
using SubclassesTracker.Caching.Services.ObjectSerilization;

namespace SubclassesTracker.Caching.Parquet
{
    public interface IDynamicParquetReader
    {
        /// <summary>
        /// Reads a Parquet file and returns a list of dictionaries representing the rows
        /// </summary>
        /// <param name="inputPath">Path to the Parquet file</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Data read from the Parquet file as a list of schema</returns>
        Task<List<T>> ReadTypedAsync<T>(
            string relativePath,
            CancellationToken ct = default)
            where T : class, new();
    }

    /// <summary>
    /// Parquet reader that reads data into dynamic dictionaries
    /// </summary>
    public class DynamicParquetReader(
        IObjectUnflattener unflattener) : IDynamicParquetReader
    {
        public async Task<List<T>> ReadTypedAsync<T>(
            string relativePath,
            CancellationToken ct = default)
            where T : class, new()
        {
            var results = new List<T>();

            using var fs = File.OpenRead(relativePath);
            using var reader = await ParquetReader.CreateAsync(fs, cancellationToken: ct);
            var schema = reader.Schema;

            for (int rg = 0; rg < reader.RowGroupCount; rg++)
            {
                using var rgReader = reader.OpenRowGroupReader(rg);
                var columns = new Dictionary<string, Array>();

                foreach (var field in schema.GetDataFields())
                    columns[field.Name] = (await rgReader.ReadColumnAsync(field, ct)).Data;

                var rowCount = columns.Values.First().Length;
                for (int i = 0; i < rowCount; i++)
                    results.Add(unflattener.DeserializeFromColumns<T>(schema, columns, i));
            }

            return results;
        }

    }
}
