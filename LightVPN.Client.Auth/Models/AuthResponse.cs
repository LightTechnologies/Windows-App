namespace LightVPN.Client.Auth.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Response from the API after a successful authentication attempt (this is also written to the auth file)
    /// </summary>
    public sealed class AuthResponse
    {
        /// <summary>
        ///     Internal user ID of the user
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        /// <summary>
        ///     The ID pointing to the new session created for the user
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        ///     Username of the user
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>
        ///     Email of the user
        /// </summary>

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
