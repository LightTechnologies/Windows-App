using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    /// <summary>
    /// Generic base response that all the APIs return with
    /// </summary>
    public class GenericResponse
    {
        /// <summary>
        /// The response code
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }
        /// <summary>
        /// The message from the API, normally this contains error information
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}