using SubclassesTracker.Caching.Entity;
using SubclassesTracker.Caching.Parquet;

namespace SubclassesTracker.Caching.Datasets
{
    public static partial class Datasets
    {
        /// <summary>
        /// Replace dimension into /base/dim/dim_players.parquet
        /// </summary>
        public static IParquetDataset<DimPlayer> DimPlayers(string partition) => new Dataset<DimPlayer>(
            name: "dim",
            mode: WriteModeEnum.ReplaceAll,
            columns: [
                Column<DimPlayer, string>.Create("report_code",   x => x.LogId),
                Column<DimPlayer, string>.Create("role",        x => x.Role),
                Column<DimPlayer, int>.Create("id",             x => x.Id),
                Column<DimPlayer, int>.Create("guid",           x => x.Guid),
                Column<DimPlayer, string>.Create("name",        x => x.Name),
                Column<DimPlayer, string>.Create("type",        x => x.Type),
                Column<DimPlayer, string>.Create("server",      x => x.Server),
                Column<DimPlayer, string>.Create("display_name",x => x.DisplayName),
                Column<DimPlayer, bool>.Create("anonymous",     x => x.Anonymous),
                Column<DimPlayer, string>.Create("icon",        x => x.Icon),
            ],
            partitionFolder: _ => partition,
            fixedFileName: null
        );

        /// <summary>
        /// Replace dimension into /base/dim/players_talent/part-*.parquet
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
        public static IParquetDataset<DimPlayerTalent> DimPlayerTalent(string partition) => new Dataset<DimPlayerTalent>(
        name: "players_talent",
        mode: WriteModeEnum.Append,
        columns: [
            Column<DimPlayerTalent, string>.Create("report_code", x => x.LogId),
            Column<DimPlayerTalent, int>.Create("player_id", x => x.PlayerId),
            Column<DimPlayerTalent, int>.Create("talent_guid", x => x.TalentGuid),
            Column<DimPlayerTalent, string?>.Create("talent_name", x => x.TalentName),
            Column<DimPlayerTalent, int?>.Create("type", x => x.Type),
            Column<DimPlayerTalent, string?>.Create("ability_icon", x => x.AbilityIcon),
            Column<DimPlayerTalent, int?>.Create("flags", x => x.Flags),
        ],
        partitionFolder: _ => partition,
        fixedFileName: null
    );
    }
}
