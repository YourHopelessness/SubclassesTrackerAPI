using SubclassesTracker.Caching.Entity;
using SubclassesTracker.Caching.Parquet;

namespace SubclassesTracker.Caching.Datasets
{
    public static partial class Datasets
    {
        /// <summary>
        /// Append facts into /base/fights/part-*.parquet
        /// </summary>
        public static IParquetDataset<FactFight> FactFights(string partition) => new Dataset<FactFight>(
            name: "fights",
            mode: WriteModeEnum.Append,
            columns: [
                Column<FactFight, string>.Create("report_code",   x => x.LogId),
                Column<FactFight, int?>.Create("zone_id",         x => x.ZoneId),
                Column<FactFight, string?>.Create("zone_name",    x => x.ZoneName),
                Column<FactFight, long>.Create("fight_id",        x => x.FightId),
                Column<FactFight, int>.Create("encounter_id",     x => x.EncounterId),
                Column<FactFight, string>.Create("encounter_name",x => x.EncounterName),
                Column<FactFight, int?>.Create("trial_score",     x => x.TrialScore),
                Column<FactFight, bool?>.Create("kill",           x => x.Kill),
            ],
            partitionFolder: _ => partition,
            fixedFileName: null
        );

        /// <summary>
        /// Replace dimension into /base/dim/dim_encounters.parquet
        /// </summary>
        public static IParquetDataset<DimEncounter> DimEncounters() => new Dataset<DimEncounter>(
            name: "dim",
            mode: WriteModeEnum.ReplaceAll,
            columns: [
                Column<DimEncounter, int>.Create("encounter_id",    x => x.EncounterId),
                Column<DimEncounter, string>.Create("encounter_name",x => x.EncounterName),
                Column<DimEncounter, int?>.Create("zone_id",        x => x.ZoneId),
                Column<DimEncounter, string?>.Create("zone_name",   x => x.ZoneName),
            ],
            partitionFolder: _ => string.Empty,
            fixedFileName: "dim_encounters.parquet"
        );
    }
}
