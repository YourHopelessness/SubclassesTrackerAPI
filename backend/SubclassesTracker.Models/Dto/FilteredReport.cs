using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Models.Dto
{
    public sealed record FilteredReport(
         string LogId,
         int ZoneId,
         string ZoneName,
         List<FightEsologsResponse> Fights);

    public sealed record FilterReportsResult(
        List<FilteredReport> WithoutCense,
        List<FilteredReport> WithCense);
}
