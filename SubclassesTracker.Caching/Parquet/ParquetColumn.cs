using Parquet.Schema;

namespace SubclassesTracker.Caching.Parquet
{
    public interface IParquetColumn<T>
    {
        /// <summary>
        /// Parquet schema field
        /// </summary>
        DataField Field { get; }

        /// <summary>
        /// Project a batch of items into a strongly-typed array matching Field
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        Array ProjectBatch(IReadOnlyList<T> batch);
    }

    public sealed class Column<T, TValue>(string name, Func<T, TValue> selector) : IParquetColumn<T>
    {
        public DataField Field { get; } = new DataField<TValue>(name);
        private readonly Func<T, TValue> _selector = selector ?? throw new ArgumentNullException(nameof(selector));

        public Array ProjectBatch(IReadOnlyList<T> batch) =>
            batch.Select(_selector).ToArray();

        public static Column<T, TValue> Create(string name, Func<T, TValue> selector) =>
            new(name, selector);
    }
}
