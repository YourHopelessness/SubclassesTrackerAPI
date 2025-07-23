namespace SubclassesTracker.Api.Models.Responses.Api
{
    /// <summary>
    /// Represents a skill model for the ESO Logs API.
    /// </summary>
    public sealed record SkillApiResponse
    {
        /// <summary>
        /// Unique identifier for the skill.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the skill.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the skill, indicating its category (like Active or Passive).
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
