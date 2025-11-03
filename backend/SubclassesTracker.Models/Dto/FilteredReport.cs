using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Models.Dto
{
    public sealed record FilteredReport(
         string LogId,
         int ZoneId,
         string ZoneName,
         List<FightEsologsResponse> Fights);
}
