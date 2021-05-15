using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    /// <summary>
    /// Contains information about a get configuration request
    /// </summary>
    public class ConfigResponse : GenericResponse
    {
        [JsonPropertyName("bytes")]
        public string ConfigArchiveBase64 { get; set; }
    }
}