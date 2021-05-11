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

using LightVPN.Common.Models;
using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    public class Server
    {
        [JsonPropertyName("countryName")]
        public string CountryName { get; set; }

        [JsonPropertyName("devicesOnline")]
        public uint DevicesOnline { get; set; }

        [JsonPropertyName("pritunlName")]
        public string FileName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("type")]
        public ServerType Type { get; set; }
    }
}