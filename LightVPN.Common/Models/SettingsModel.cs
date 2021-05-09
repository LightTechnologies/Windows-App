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

namespace LightVPN.Common.Models
{
    public class PreviousServer
    {
        public string Id { get; set; }

        public string ServerName { get; set; }

        public ServerType Type { get; set; }
    }

    public class SettingsModel
    {
        public bool AutoConnect { get; set; }

        public bool DarkMode { get; set; }

        public bool DiscordRpc { get; set; } = true;

        public bool KillSwitch { get; set; }

        public PreviousServer PreviousServer { get; set; }

        public SizeSaving SizeSaving { get; set; }
    }

    public class SizeSaving
    {
        public uint Height { get; set; } = 420;

        public bool IsSavingSize { get; set; }

        public uint Width { get; set; } = 550;
    }
}