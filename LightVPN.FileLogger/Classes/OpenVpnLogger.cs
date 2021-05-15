/* --------------------------------------------
 *
 * File logger - OpenVPN Logger
 * Copyright (C) Light Technologies LLC
 *
 * File: OpenVpnLogger.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.Common.Models;
using LightVPN.FileLogger.Base;

namespace LightVPN.FileLogger
{
    /// <summary>
    /// Class used to write OpenVPN output to the OpenVPN log file
    /// </summary>
    public class OpenVpnLogger : FileLoggerBase
    {
        public OpenVpnLogger() : base(Globals.OpenVpnLogPath)
        {
        }
    }
}