namespace SubclassesTracker.Caching.Entity
{
    /// <summary>
    /// Represents a player in the DimPlayer dimension table.
    /// </summary>
    /// <param name="LogId">specific report code</param>
    /// <param name="FightId">specific fight id</param>
    /// <param name="Role">Player's role "Tank" | "Healer" | "Dps"</param>
    /// <param name="Id">identifier of the player</param>
    /// <param name="Guid">internal unique identifier of the player</param>
    /// <param name="Name">player's name</param>
    /// <param name="Type">player type</param>
    /// <param name="Server">player megaserver e.g. NA | EU </param>
    /// <param name="DisplayName">display player's name</param>
    /// <param name="Anonymous">is anonymous</param>
    /// <param name="Icon">player icon</param>
    public sealed record DimPlayer(
        string LogId,
        int FightId,
        string Role,
        int Id,
        int Guid,
        string Name,
        string Type,
        string Server,
        string DisplayName,
        bool Anonymous,
        string Icon
    );
}
