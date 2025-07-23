using Newtonsoft.Json;

namespace SubclassesTracker.Api.Models.Responses.Esologs
{
    public sealed record CombatInfoEsologsResponse
    {
        /// <summary>
        /// Skills
        /// </summary>
        [JsonProperty("talents")]
        public List<PlayerSkillsEsologsResponse> Talents { get; set; } = [];
    }
}
