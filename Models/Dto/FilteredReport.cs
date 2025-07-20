using SubclassesTracker.Api.Models.Responses.Esologs;

namespace SubclassesTracker.Api.Models.Dto
{
    public sealed record FilteredReport(
         string LogId,
         int ZoneId,
         string ZoneName,
         List<FightEsologsResponse> Fights);
}
