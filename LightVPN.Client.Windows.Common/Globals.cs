using System;
using System.IO;

namespace LightVPN.Client.Windows.Common
{
    public static class Globals
    {
        private static readonly string AppSettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN");

        public static readonly string AppCachePath = Path.Combine(AppSettingsDirectory, "VPNCache");
        public static readonly string OpenVpnPath = Path.Combine(AppSettingsDirectory, "OpenVPN");
        public static readonly string OpenVpnDriversPath = Path.Combine(AppSettingsDirectory, "OpenVPNDrivers");
        public static readonly string AppSettingsPath = Path.Combine(AppSettingsDirectory, "config.json");
        public static readonly string AuthDataPath = Path.Combine(AppSettingsDirectory, "auth.bin");
        public static readonly string OpenVpnLogPath = Path.Combine(AppSettingsDirectory, "ovpn.log");
    }
}