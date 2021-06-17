using System;

namespace LightVPN.Client.Auth.Utils
{
    internal static class Extensions
    {
        internal static string ConvertPlatformToString(this PlatformID platform)
        {
            return platform switch
            {
                PlatformID.Win32S => "Win9x",
                PlatformID.Win32Windows => "Win9x",
                PlatformID.Win32NT => "Windows",
                PlatformID.WinCE => "WindowsM",
                PlatformID.Unix => "Linux",
                PlatformID.Xbox => "Xbox",
                PlatformID.MacOSX => "macOS",
                PlatformID.Other => "Other",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}