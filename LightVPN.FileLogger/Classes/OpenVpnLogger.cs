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
    public class OpenVpnLogger : FileLoggerBase
    {
        public OpenVpnLogger() : base(Globals.OpenVpnLogPath)
        {
        }
    }
}