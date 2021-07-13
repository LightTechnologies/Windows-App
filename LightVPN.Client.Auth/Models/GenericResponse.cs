namespace LightVPN.Client.Auth.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Generic failure / non-failure response from the API
    /// </summary>
    public sealed class GenericResponse
    {
        /// <summary>
        ///     The error code
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        ///     The message (normally what has gone wrong in the request)
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
