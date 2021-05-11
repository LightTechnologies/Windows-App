using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    public class GenericResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}