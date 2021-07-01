using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;
using LightVPN.Client.Debug;
using Microsoft.Win32;

namespace LightVPN.Client.Windows.Common.Utils
{
    /// <summary>
    ///     Class that handles Windows registry related stuff, this is unused at the moment due to Windows startup based
    ///     restrictions
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class StartupHelper
    {
        /// <summary>
        ///     Disables the LightVPN client to run on startup
        /// </summary>
        public static void DisableRunOnStartup()
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (IsRunningOnStartup()) regKey?.DeleteValue("LightVPN", true);
        }

        /// <summary>
        ///     Enables the LightVPN client to run on startup via the registry
        /// </summary>
        public static void EnableRunOnStartup(string executablePath)
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (!IsRunningOnStartup())
                regKey?.SetValue("LightVPN", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"startupHelper.exe {executablePath}"),
                    RegistryValueKind.String);
        }

        /// <summary>
        ///     Checks whether the LightVPN client is configured to run on startup
        /// </summary>
        /// <returns>True or false value whether the client is configured or not</returns>
        public static bool IsRunningOnStartup()
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            var result = regKey?.GetValue("LightVPN") is not null;

            DebugLogger.Write("lvpn-client-win-common-startuphelper", $"conditional result for reg-key: {result}");

            return result;
        }
    }
}
