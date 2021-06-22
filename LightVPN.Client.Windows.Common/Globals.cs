using System;
using System.IO;
using SimpleInjector;

namespace LightVPN.Client.Windows.Common
{
    /// <summary>
    ///     Contains global variables for most classes to access (as long as it's not a CIRCULAR DEPENDENCY)
    /// </summary>
    public static class Globals
    {
        /// <summary>
        ///     Directory where everything LightVPN is stored
        /// </summary>
        private static readonly string AppSettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN");

        /// <summary>
        ///     Path to where server configuration cache is stored
        /// </summary>
        public static readonly string AppCachePath = Path.Combine(AppSettingsDirectory, "VPNCache");

        /// <summary>
        ///     Path to the OpenVPN binaries
        /// </summary>
        public static readonly string OpenVpnPath = Path.Combine(AppSettingsDirectory, "OpenVPN");

        /// <summary>
        ///     Path to the TAP utility binary
        /// </summary>
        public static readonly string TapCtlPath = Path.Combine(OpenVpnPath, "tapctl.exe");

        /// <summary>
        ///     Path to the OpenVPN TAP drivers
        /// </summary>
        public static readonly string OpenVpnDriversPath = Path.Combine(AppSettingsDirectory, "OpenVPNDrivers");

        /// <summary>
        ///     Path to the LightVPN configuration
        /// </summary>
        public static readonly string AppSettingsPath = Path.Combine(AppSettingsDirectory, "config.json");

        /// <summary>
        ///     Path to the LightVPN authentication data file
        /// </summary>
        public static readonly string AuthDataPath = Path.Combine(AppSettingsDirectory, "auth.bin");

        /// <summary>
        ///     Path to the OpenVPN logs
        /// </summary>
        public static readonly string OpenVpnLogPath = Path.Combine(AppSettingsDirectory, "ovpn.log");

        /// <summary>
        ///     SimpleInjector DI container
        /// </summary>
        public static readonly Container Container = new();

        /// <summary>
        ///     Instance of a login window
        /// </summary>
        public static object LoginWindow;

        /// <summary>
        ///     Username of the currently signed in user
        /// </summary>
        public static string UserName;

        /// <summary>
        ///     True if this build is a beta build, false if it's stable
        /// </summary>
        public static bool IsBeta { get; set; } = true;
    }
}