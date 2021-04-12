﻿/* --------------------------------------------
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
    public struct Server
    {
        [JsonProperty("id")] 
        public string Id { get; set; }
        [JsonProperty("serverName")]
        public string ServerName { get; set; }

        [JsonProperty("location")] 
        public string Location { get; set; }

        [JsonProperty("type")]
        public ServerType Type { get; set; }

        [JsonProperty("countryName")]
        public string Country { get; set; }

        [JsonProperty("pritunlName")] 
        public string FileName { get; set; }
    }
    public enum ServerType
    {
        Regular,
        Speed,
        Privacy,
        Game
    }
}