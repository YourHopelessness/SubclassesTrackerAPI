namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents an aura (buff) event in a game, capturing details such as the timestamp, fight ID, source ID,
    /// </summary>
    /// <param name="LogId">specific report code</param>
    /// <param name="Timestamp">timestamp</param>
    /// <param name="FightIds">specific fight ids</param>
    /// <param name="SourceId">applyer of the buff</param>
    /// <param name="AbilityGuid">buff identidier</param>
    /// <param name="Stacks">count of the stacts of applied</param>
    /// <param name="AbilityName">buff name</param>
    /// <param name="Icon">buff icon</param>
    public sealed record FactAura(
         string LogId,
         long Timestamp,
         int[] FightIds,
         int SourceId,
         int AbilityGuid,
         int Stacks,
         string? AbilityName,
         string? Icon
    );
}
