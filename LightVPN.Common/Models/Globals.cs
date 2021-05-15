/* --------------------------------------------
 *
 * Globals - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: Globals.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using SimpleInjector;
using System;
using System.IO;
using System.Runtime.Versioning;

namespace LightVPN.Common.Models
{
    /// <summary>
    /// This class contains properties that are accessed by every class usually
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Path to where all the settings and cache is stored
        /// </summary>
        public static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN");
        /// <summary>
        /// Path to the authentication data file
        /// </summary>
        public static readonly string AuthPath = Path.Combine(SettingsPath, "auth.bin");
        /// <summary>
        /// Path to where the VPN server cache is kept
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(SettingsPath, "cache");
        /// <summary>
        /// Dependency injection container, contains all our dependencies
        /// </summary>
        public static readonly Container Container = new();
        /// <summary>
        /// Path to the error log file
        /// </summary>
        public static readonly string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "error.log");
        /// <summary>
        /// Path to the drivers for the OpenVPN TAP adapter
        /// </summary>
        public static readonly string OpenVpnDriversPath = Path.Combine(SettingsPath, "drivers");
        /// <summary>
        /// Path to the OpenVPN log file
        /// </summary>
        public static readonly string OpenVpnLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn.log");
        /// <summary>
        /// Path to where the OpenVPN binaries are kept
        /// </summary>
        public static readonly string OpenVpnPath = Path.Combine(SettingsPath, "ovpn");
        /// <summary>
        /// Path to the settings file
        /// </summary>
        public static readonly string SettingsFile = Path.Combine(SettingsPath, "config.bin");

        // This is so it saves settings in the users home folder (THESE VARS ARE FOR LINUX CLI USE)
        /// <summary>
        /// Path to where all the settings and cache is stored, this is Linux specific
        /// </summary>
        public static readonly string LinuxSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".lvpn");
        /// <summary>
        /// Path to the authentication data file, this is Linux specific
        /// </summary>
        public static readonly string LinuxAuthPath = Path.Combine(LinuxSettingsPath, "auth.bin");
        /// <summary>
        /// Stores the Session ID for the current user, this is Linux specific
        /// </summary>
        public static Guid LinuxSessionId = Guid.Empty;
        /// <summary>
        /// Path to where the VPN server cache is kept, this is Linux specific
        /// </summary>
        public static readonly string LinuxConfigPath = Path.Combine(LinuxSettingsPath, "cache");
        /// <summary>
        /// Path to where the OpenVPN TAP drivers are kept, this is unused since the OpenVPN package already depends upon these
        /// </summary>
        public static readonly string LinuxOpenVpnDriversPath = Path.Combine(LinuxSettingsPath, "drivers");
        /// <summary>
        /// Path to where the OpenVPN binaries are kept, this is unused since the Linux CLI depends upon the OpenVPN package
        /// </summary>
        public static readonly string LinuxOpenVpnPath = Path.Combine(LinuxSettingsPath, "ovpn");
        /// <summary>
        /// Whether the app has been minimized to the system tray or not
        /// </summary>
        public static bool IsMinimizedToTray;
    }
}