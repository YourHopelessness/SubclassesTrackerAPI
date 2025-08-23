using SubclassesTracker.Caching.Entity;
using SubclassesTracker.Caching.Parquet;
using System.Collections.Concurrent;

namespace SubclassesTracker.Caching.Datasets
{
    public static partial class Datasets
    {
        /// <summary>
        /// Append into /base/eventbuffs/part-*.parquet
        /// </summary>
        public static IParquetDataset<FactAura> FactAuras(string partition) => new Dataset<FactAura>(
            name: "eventbuffs",
            mode: WriteModeEnum.Append,
            columns: [
                Column<FactAura, string>.Create("report_code",   x => x.LogId),
                Column<FactAura, long>.Create("ts",             x => x.Timestamp),
                Column<FactAura, int[]>.Create("fight_id",        x => x.FightIds),
                Column<FactAura, int>.Create("source_id",       x => x.SourceId),
                Column<FactAura, int>.Create("ability_guid",    x => x.AbilityGuid),
                Column<FactAura, int>.Create("stacks",          x => x.Stacks),
                Column<FactAura, string?>.Create("ability_name",x => x.AbilityName),
                Column<FactAura, string?>.Create("icon",        x => x.Icon),
            ],
            partitionFolder: _ => partition,
            fixedFileName: null
        );

        /// <summary>
        /// Replace dimension into /base/buffs/part-*.parquet
        /// </summary>
        public static IParquetDataset<DimBuff> DimBuffs(string partition) => new Dataset<DimBuff>(
            name: "buffs",
            mode: WriteModeEnum.ReplaceAll,
            columns: [
                Column<DimBuff, string>.Create("report_code",   x => x.LogId),
                Column<DimBuff, int[]>.Create("fight_ids",        x => x.FightIds),
                Column<DimBuff, int>.Create("buff_guid",         x => x.Guid),
                Column<DimBuff, string>.Create("buff_name",      x => x.Name),
            ],
            partitionFolder: _ => partition,
            fixedFileName: null
        );
    }
}
