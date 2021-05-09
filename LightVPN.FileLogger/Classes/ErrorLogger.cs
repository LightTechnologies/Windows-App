/* --------------------------------------------
 *
 * File logger - Error logger
 * Copyright (C) Light Technologies LLC
 *
 * File: ErrorLogger.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.Common.Models;
using LightVPN.FileLogger.Base;

namespace LightVPN.FileLogger
{
    public class ErrorLogger : FileLoggerBase
    {
        public ErrorLogger() : base(Globals.ErrorLogPath)
        {
        }
    }
}