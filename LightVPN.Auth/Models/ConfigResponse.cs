using Newtonsoft.Json;

namespace LightVPN.Auth.Models
{
    public class ConfigResponse : GenericResponse
    {
        [JsonProperty("bytes")]
        public string ConfigArchiveBase64 { get; set; }
    }
}