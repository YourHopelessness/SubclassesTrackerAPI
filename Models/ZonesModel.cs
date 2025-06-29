namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a zone in the game, which contains encounters.
    /// </summary>
    public class ZoneModel
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
        public List<EncounterModel> Encounters { get; set; } = new();
    }
}
