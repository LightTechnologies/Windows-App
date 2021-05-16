using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    /// <summary>
    /// Contains data about a changelog request
    /// </summary>
    public class Changelog : GenericResponse
    {

        [JsonPropertyName("changelog")]
        public string Content { get; set; }
    }
}