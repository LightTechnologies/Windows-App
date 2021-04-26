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

using Newtonsoft.Json;

namespace LightVPN.Common.Models
{
    public class SettingsModel
    {
        public bool AutoConnect { get; set; }
        public bool DarkMode { get; set; }
        public bool KillSwitch { get; set; }
        public PreviousServer PreviousServer { get; set; }
        public bool DiscordRpc { get; set; } = true;

    }

    public class PreviousServer
    {
        public string Id { get; set; }
        public string ServerName { get; set; }
        public ServerType Type { get; set; }
    }
}
