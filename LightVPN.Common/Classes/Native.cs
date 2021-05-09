﻿/* --------------------------------------------
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

using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using LightVPN.Common.Interfaces;
using Microsoft.Win32;

namespace LightVPN.Common
{
    public class Native : INative
    {
        private static RegistryKey _regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        /// <summary>
        /// Disables the LightVPN client to run on startup
        /// </summary>
        [SupportedOSPlatform("windows")]
        public void DisableRunOnStartup()
        {
            if (IsRunningOnStartup())
            {
                _regKey.DeleteValue("LightVPN", true);
            }
        }

        public void Dispose()
        {
            _regKey.Close();
            _regKey.Dispose();
            _regKey = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Enables the LightVPN client to run on startup via the registry
        /// </summary>
        [SupportedOSPlatform("windows")]
        public void EnableRunOnStartup()
        {
            if (!IsRunningOnStartup())
            {
                _regKey.SetValue("LightVPN", Process.GetCurrentProcess().MainModule.FileName, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Checks whether the LightVPN client is configured to run on startup
        /// </summary>
        /// <returns>True or false value whether the client is configured or not</returns>
        [SupportedOSPlatform("windows")]
        public bool IsRunningOnStartup()
        {
            var subKey = _regKey.GetValue("LightVPN");
            if (subKey is null) return false;
            return true;
        }
    }
}