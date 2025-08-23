namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents a buff with its unique identifier, name, and optional class skill line association.
    /// </summary>
    /// <param name="LogId">specific report code</param>
    /// <param name="FightIds">specific fight id</param>
    /// <param name="Guid">Buff guid</param>
    /// <param name="Name">buff name</param>
    public sealed record DimBuff(
        string LogId,
        int[] FightIds,
        int Guid,
        string Name
    );
}
