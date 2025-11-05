using Newtonsoft.Json;

namespace SubclassesTracker.Models.Responses.Esologs
{
    public sealed record PlayerSkillsEsologsResponse
    {
        /// <summary>
        /// Id of skill
        /// </summary>
        [JsonProperty("guid")]
        public int Id { get; set; }
        /// <summary>
        /// Name of the skill
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
