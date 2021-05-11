/* --------------------------------------------
 *
 * Configuration file - Model
 * Copyright (C) Light Technologies LLC
 *
 * File: Configuration.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using System.Text.Json.Serialization;

namespace LightVPN.Common.Models
{
    public class PreviousServer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        [JsonPropertyName("type")]
        public ServerType Type { get; set; }
    }

    public class SettingsModel
    {
        [JsonPropertyName("autoConnect")]
        public bool AutoConnect { get; set; }

        [JsonPropertyName("darkMode")]
        public bool DarkMode { get; set; }

        [JsonPropertyName("discordRpc")]
        public bool DiscordRpc { get; set; } = true;

        [JsonPropertyName("killSwitch")]
        public bool KillSwitch { get; set; }

        [JsonPropertyName("previousServer")]
        public PreviousServer PreviousServer { get; set; }

        [JsonPropertyName("sizeSaving")]
        public SizeSaving SizeSaving { get; set; }
    }

    public class SizeSaving
    {
        [JsonPropertyName("height")]
        public uint Height { get; set; } = 420;

        [JsonPropertyName("isSavingSize")]
        public bool IsSavingSize { get; set; }

        [JsonPropertyName("width")]
        public uint Width { get; set; } = 550;
    }
}