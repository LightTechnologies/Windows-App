namespace LightVPN.Client.Windows.Common
{
    using System;
    using System.IO;
    using SimpleInjector;

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
        public static readonly string AppCachePath = Path.Combine(Globals.AppSettingsDirectory, "Cache");

        public static readonly string AppOpenVpnCachePath = Path.Combine(Globals.AppCachePath, "OpenVPN");

        public static readonly string AppServerCachePath = Path.Combine(Globals.AppCachePath, "API");

        /// <summary>
        ///     Path to the OpenVPN binaries
        /// </summary>
        public static readonly string OpenVpnPath = Path.Combine(Globals.AppSettingsDirectory, "OpenVPN");

        /// <summary>
        ///     Path to the TAP utility binary
        /// </summary>
        public static readonly string TapCtlPath = Path.Combine(Globals.OpenVpnPath, "tapctl.exe");

        /// <summary>
        ///     Path to the OpenVPN TAP drivers
        /// </summary>
        public static readonly string OpenVpnDriversPath = Path.Combine(Globals.AppSettingsDirectory, "OpenVPNDrivers");

        /// <summary>
        ///     Path to the LightVPN configuration
        /// </summary>
        public static readonly string AppSettingsPath = Path.Combine(Globals.AppSettingsDirectory, "config.json");

        /// <summary>
        ///     Path to the LightVPN authentication data file
        /// </summary>
        public static readonly string AuthDataPath = Path.Combine(Globals.AppSettingsDirectory, "auth.bin");

        /// <summary>
        ///     Path to the OpenVPN logs
        /// </summary>
        public static readonly string OpenVpnLogPath = Path.Combine(Globals.AppSettingsDirectory, "ovpn.log");

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

        /// <summary>
        ///     Instance of a TrayViewModel so we can modify its properties from other ViewModels
        /// </summary>
        public static object TrayViewModel { get; set; }

        /// <summary>
        ///     Instance of a MainViewModel so we can modify its properties from other ViewModels
        /// </summary>
        public static object MainViewModel { get; set; }

        /// <summary>
        ///     True if the MainWindow is in the tray, false otherwise.
        /// </summary>
        public static bool IsInTray { get; set; }

        public static bool IsStartingMinimised { get; set; }
    }
}
