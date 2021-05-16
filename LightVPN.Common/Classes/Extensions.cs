/* --------------------------------------------
 *
 * Extension methods - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: Extensions.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace LightVPN.Common
{
    /// <summary>
    /// Contains various extension methods that are usually used by most classes
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the assembly and returns the version
        /// </summary>
        /// <param name="assembly">The assembly, should be the executing assembly</param>
        /// <returns>The version of the executing assembly</returns>
        [Obsolete("Use the reflection based method instead, this won't work because of single file")]
        public static Version GetVersion(this Assembly assembly) => Version.Parse(FileVersionInfo.GetVersionInfo(Path.GetFullPath(assembly.Location)).FileVersion);

        /// <summary>
        /// Attempts to parse <paramref name="strInput"/> as <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The object you want to deserialize <paramref name="strInput"/> to</typeparam>
        /// <param name="strInput">The input you want to validate</param>
        /// <returns>True if we could parse, false otherwise</returns>
        public static bool IsValidJson<T>(this string strInput)
        {
            strInput = strInput.Trim();
            try
            {
                JsonSerializer.Deserialize<T>(strInput);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a TimeSpan to a string, hours, mins, seconds
        /// </summary>
        /// <param name="time">TimeSpan to be converted</param>
        /// <returns>The formatted string</returns>
        public static string ToHMS(this TimeSpan time)
        {
            string converted = null;
            converted += time.Days == 0 ? "" : time.Days == 1 ? $"{time.Days} day " : $"{time.Days} days ";
            converted += time.Hours == 0 ? "" : time.Hours == 1 ? $"{time.Hours} hour " : $"{time.Hours} hours ";
            converted += time.Minutes == 0 ? "" : time.Seconds == 0 ? time.Minutes == 1 ? $"{time.Minutes} minute " : $"{time.Minutes} minutes " : time.Minutes == 1 ? $"{time.Minutes} minute and " : $"{time.Minutes} minutes and ";
            converted += time.Seconds == 0 ? "" : time.Seconds == 1 ? $"{time.Seconds} second" : $"{time.Seconds} seconds";
            return converted;
        }
    }
}