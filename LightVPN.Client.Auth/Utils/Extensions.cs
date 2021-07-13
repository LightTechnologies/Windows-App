namespace LightVPN.Client.Auth.Utils
{
    using System;

    internal static class Extensions
    {
        /// <summary>
        ///     Converts a PlatformID enum to a string for the X-Client-Version API header
        /// </summary>
        /// <param name="platform">The platform ID</param>
        /// <returns>The string for the platform passed in</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the PlatformID is unrecognised (out-of-range)</exception>
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
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
            };
        }
    }
}
