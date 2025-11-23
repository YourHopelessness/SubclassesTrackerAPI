namespace SubclassesTracker.Models.Responses.Api
{
    /// <summary>
    /// Represents a zone in the game, which contains encounters.
    /// </summary>
    public sealed record ZoneApiResponse
    {
        /// <summary>
        /// Unique identifier for the zone.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the zone.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Collection of encounters in the zone.
        /// </summary>
        public List<EncounterApiResponse> Encounters { get; set; } = new();
    }
}
