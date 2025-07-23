using SubclassesTracker.Api.Models.Enums;

namespace SubclassesTracker.Api.Models.Dto
{
    public sealed record GroupKey(
        string PlayerName,
        int TrialId,
        string TrialName,
        string PlayerEsoId,
        string SpecKey,
        PlayerRole Role);
}
