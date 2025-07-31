using static SubclassesTracker.Api.EsologsServices.Reports.ReportSubclassesDataService;

namespace SubclassesTracker.Api.Models.Dto
{
    /// <summary>
    /// Represents a row of player data in the report.
    /// </summary>
    public sealed record PlayerRow(
        int PlayerId,
        string LogId,
        List<int> FightIds,
        string Role,
        int TrialId,
        string TrialName,
        List<Talent> Talents,
        string BaseClass);
}
