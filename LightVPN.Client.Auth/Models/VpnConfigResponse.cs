namespace LightVPN.Client.Auth.Models
{
    using System.Text.Json.Serialization;

    public sealed class VpnConfigResponse
    {
        [JsonPropertyName("bytes")] public string ArchiveBase64 { get; set; }
    }
}
