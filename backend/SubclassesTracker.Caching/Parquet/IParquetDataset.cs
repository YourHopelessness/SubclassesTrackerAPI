using SubclassesTracker.Models.Enums;
using System.Reflection;

namespace SubclassesTracker.Caching.Parquet
{
    public interface IParquetDataset<T>
    {
        /// <summary>
        /// Dataset name (used in path, e.g., "fights", "dim", "auras")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Column definitions (order defines physical column order)
        /// </summary>
        IReadOnlyList<IParquetColumn<T>> Columns { get; }

        /// <summary>
        /// Decide where to place a single row: relative partition folder (e.g., "period=2025-07")
        /// </summary>
        /// <returns>Return string.Empty if no partitioning is needed.</returns>
        Func<T, string> PartitionFolder { get; }

        // Append vs. ReplaceAll
        WriteModeEnum Mode { get; }

        /// <summary>
        /// When Mode == ReplaceAll, a fixed file name to overwrite (e.g., "dim_buffs.parquet")
        /// </summary>
        string? FixedFileName { get; }
    }

    public class AutoParquetDataset<T>(
        string name,
        WriteModeEnum mode = WriteModeEnum.Append,
        Func<T, string>? partitionFunc = null,
        string? fixedFileName = null) : IParquetDataset<T>
    {
        public string Name { get; } = name;
        public IReadOnlyList<IParquetColumn<T>> Columns { get; } = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select(ParquetColumn<T>.FromProperty)
                .ToList();
        public Func<T, string> PartitionFolder { get; } = partitionFunc ?? (_ => string.Empty);
        public WriteModeEnum Mode { get; } = mode;
        public string? FixedFileName { get; } = fixedFileName;
    }

}
