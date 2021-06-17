using System.Text.Json.Serialization;

namespace LightVPN.Client.Auth.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("userId")] public string UserId { get; set; }
        [JsonPropertyName("sessionId")] public string SessionId { get; set; }

        [JsonPropertyName("username")] public string UserName { get; set; }

        [JsonPropertyName("email")] public string Email { get; set; }
    }
}