/* --------------------------------------------
 * 
 * Server object - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: Servers.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using Newtonsoft.Json;

namespace LightVPN.Auth.Models
{
    public struct Servers
    {
        [JsonProperty("id")] 
        public string Id { get; set; }

        [JsonProperty("location")] 
        public string Location { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("country_name")]
        public string Country { get; set; }

        [JsonProperty("pritunl_name")] 
        public string FileName { get; set; }
    }
}
