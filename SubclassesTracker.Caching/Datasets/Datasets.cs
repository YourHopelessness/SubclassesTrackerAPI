using SubclassesTracker.Caching.Parquet;

namespace SubclassesTracker.Caching.Datasets
{
    public sealed class Dataset<T>(
        string name,
        WriteModeEnum mode,
        IReadOnlyList<IParquetColumn<T>> columns,
        Func<T, string> partitionFolder,
        string? fixedFileName) : IParquetDataset<T>
    {
        public string Name { get; } = name;
        public IReadOnlyList<IParquetColumn<T>> Columns { get; } = columns;
        public Func<T, string> PartitionFolder { get; } = partitionFolder;
        public WriteModeEnum Mode { get; } = mode;
        public string? FixedFileName { get; } = fixedFileName;
    }
}
