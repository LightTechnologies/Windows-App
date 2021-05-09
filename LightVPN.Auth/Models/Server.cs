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
using Newtonsoft.Json;

namespace LightVPN.Auth.Models
{
    public struct Server
    {
        public string CountryName { get; set; }

        public long DevicesOnline { get; set; }

        [JsonProperty("pritunlName")]
        public string FileName { get; set; }

        public string Id { get; set; }

        public string Location { get; set; }

        public string ServerName { get; set; }

        public bool Status { get; set; }

        public ServerType Type { get; set; }
    }
}