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

namespace LightVPN.Common.v2.Models
{
    public static class Globals
    {
        public static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN");
        public static readonly string OpenVpnLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn.log");
        public static readonly string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "error.log");
        public static readonly string SettingsFile = Path.Combine(SettingsPath, "config.bin");
        public static readonly string ConfigPath = Path.Combine(SettingsPath, "cache");
        public static readonly string OpenVpnPath = Path.Combine(SettingsPath, "ovpn");
        public static readonly string OpenVpnDriversPath = Path.Combine(SettingsPath, "drivers");
        public static readonly string AuthPath = Path.Combine(SettingsPath, "auth.bin");
        public static string OpenVpnUsername { get; set; }
        public static string OpenVpnPassword { get; set; }
        public static readonly Container container = new();
    }
}
