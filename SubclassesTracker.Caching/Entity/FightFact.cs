namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents a record of a fight, including details about the encounter, zone, and outcome.
    /// </summary>
    /// <remarks>This type encapsulates information about a fight, such as the associated zone, encounter, and
    /// trial score. It is designed to provide a structured representation of fight data, including optional fields for
    /// nullable values.</remarks>
    /// <param name="LogId">The unique identifier for the log associated with the fight.</param>
    /// <param name="ZoneId">The identifier for the zone where the fight took place.</param>
    /// <param name="ZoneName">The name of the zone where the fight took place. This field is optional and may be null.</param>
    /// <param name="FightId">The unique identifier for the fight.</param>
    /// <param name="EncounterId">The identifier for the encounter associated with the fight.</param>
    /// <param name="EncounterName">The name of the encounter associated with the fight. This field is optional and may be null.</param>
    /// <param name="TrialScore">The trial score achieved during the fight. This field is optional and may be null.</param>
    /// <param name="Kill">Indicates whether the fight resulted in a kill. This field is optional and may be null.</param>
    public sealed record FactFight(
        string LogId,
        int ZoneId,
        string? ZoneName,
        long FightId,
        int EncounterId,
        string? EncounterName,
        int? TrialScore,
        bool? Kill
    );
}
