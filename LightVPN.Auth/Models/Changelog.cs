using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    /// <summary>
    /// Contains data about a changelog request
    /// </summary>
    public class Changelog
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("changelog")]
        public string Content { get; set; }
    }
}