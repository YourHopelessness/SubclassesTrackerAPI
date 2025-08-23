namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents a player's talent in a specific log, including its unique identifier, name, type, icon, and flags.
    /// </summary>
    /// <param name="LogId">specific report code</param>
    /// <param name="PlayerId">player id</param>
    /// <param name="TalentGuid">talent unique identifier</param>
    /// <param name="TalentName">talent name</param>
    /// <param name="Type">talent type</param>
    /// <param name="AbilityIcon">icon</param>
    public sealed record DimPlayerTalent(
        string LogId,
        int FightId,
        int PlayerId,
        int TalentGuid,
        string? TalentName,
        int? Type,
        string? AbilityIcon,
        int? Flags
    );
}
