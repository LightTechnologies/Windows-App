using Newtonsoft.Json;

namespace LightVPN.Auth.Models
{
    public struct Changelog
    {
        public int Code { get; set; }

        [JsonProperty("changelog")]
        public string Content { get; set; }
    }
}