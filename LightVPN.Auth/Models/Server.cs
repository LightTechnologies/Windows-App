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
    /// <summary>
    /// Contains data about a server retrieved from the API
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The country the server is located in
        /// </summary>
        [JsonPropertyName("countryName")]
        public string CountryName { get; set; }
        /// <summary>
        /// The amount of devices connected to the server
        /// </summary>
        [JsonPropertyName("devicesOnline")]
        public uint DevicesOnline { get; set; }
        /// <summary>
        /// The file name for the server, used to locate the configuration file
        /// </summary>
        [JsonPropertyName("pritunlName")]
        public string FileName { get; set; }
        /// <summary>
        /// The internal ID for the server, this is never used
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// The location string of the server, this is normally the city or state
        /// </summary>
        [JsonPropertyName("location")]
        public string Location { get; set; }
        /// <summary>
        /// This is... I forgot
        /// </summary>
        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }
        /// <summary>
        /// Whether the server is online or not
        /// </summary>
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        /// <summary>
        /// The type of server, this could be Game, Regular, etc.
        /// </summary>
        [JsonPropertyName("type")]
        public ServerType Type { get; set; }
    }
}