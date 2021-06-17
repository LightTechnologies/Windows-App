using System.Text.Json.Serialization;

namespace LightVPN.Client.Auth.Models
{
    public sealed class GenericResponse
    {
        [JsonPropertyName("code")] public int Code { get; set; }

        [JsonPropertyName("message")] public string Message { get; set; }
    }
}