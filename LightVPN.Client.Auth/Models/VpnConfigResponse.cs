using System.Text.Json.Serialization;

namespace LightVPN.Client.Auth.Models
{
    public class VpnConfigResponse
    {
        [JsonPropertyName("bytes")] public string ArchiveBase64 { get; set; }
    }
}