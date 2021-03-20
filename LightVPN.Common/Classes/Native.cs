/* --------------------------------------------
 * 
 * Native methods - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: Native.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using System.Diagnostics;
using LightVPN.Common.Interfaces;
using Microsoft.Win32;

namespace LightVPN.Common
{
    public class Native : INative
    {
        /// <summary>
        /// Enables the LightVPN client to run on startup via the registry
        /// </summary>
        public void EnableRunOnStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (!IsRunningOnStartup())
            {
                rk.SetValue("LightVPN", Process.GetCurrentProcess().MainModule.FileName, RegistryValueKind.String);
            }
        }
        /// <summary>
        /// Disables the LightVPN client to run on startup
        /// </summary>
        public void DisableRunOnStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (IsRunningOnStartup())
            {
                rk.DeleteValue("LightVPN", true);
            }
        }
        /// <summary>
        /// Checks whether the LightVPN client is configured to run on startup
        /// </summary>
        /// <returns>True or false value whether the client is configured or not</returns>
        public bool IsRunningOnStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            var subKey = rk.GetValue("LightVPN");
            if (subKey is null) return false;
            return true;
        }
    }
}
