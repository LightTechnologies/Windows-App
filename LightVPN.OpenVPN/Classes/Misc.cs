/* --------------------------------------------
 *
 * OpenVPN Manager - Miscellaneous tools
 * Copyright (C) Light Technologies LLC
 *
 * File: Misc.cs
 *
 * Created: 04-03-21 Toshi
 *
 * --------------------------------------------
 */

using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace LightVPN.OpenVPN
{
    public static class Misc
    {
        /// <summary>
        /// Checks if the TAP adapter is installed
        /// </summary>
        /// <returns>True or false whether the adapter was found or not</returns>
        [Obsolete("Use the TAP Manager class instead")]
        public static bool IsTAPInstalled()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return interfaces.Any(x => x.Description.Contains("TAP-Windows Adapter"));
            }
            return false;
        }
    }
}