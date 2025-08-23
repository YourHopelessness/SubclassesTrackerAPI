namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents an encounter within a specific zone, including its unique identifier, name, and optional zone
    /// details.
    /// </summary>
    /// <param name="EncounterId">The unique identifier for the encounter.</param>
    /// <param name="EncounterName">The name of the encounter. This value cannot be null.</param>
    /// <param name="ZoneId">The unique identifier of the zone where the encounter occurs, or <see langword="null"/> if the zone is
    /// unspecified.</param>
    /// <param name="ZoneName">The name of the zone where the encounter occurs, or <see langword="null"/> if the zone is unspecified.</param>
    public sealed record DimEncounter(
        int EncounterId,
        string EncounterName,
        int? ZoneId,
        string? ZoneName
    );
}
