using System.Text.Json.Serialization;

namespace SubclassesTracker.Api.Models.Responses.Api
{
    /// <summary>
    /// Player skillines response model
    /// </summary>
    public class PlayerSkilllinesApiResponse
    {
        /// <summary>
        /// Player's displayed character name
        /// </summary>
        public string PlayerCharacterName { get; set; } = null!;
        /// <summary>
        /// Player's ESO id
        /// </summary>
        public string PlayerEsoId { get; set; } = null!;
        /// <summary>
        /// Player's lines
        /// </summary>
        public List<PlayerSkillLine> PlayerSkillLines { get; set; } = [];
    }

    public record PlayerSkillLine(string LineName, string LineIcon);
}
