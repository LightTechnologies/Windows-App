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
    /// <summary>
    /// Class that writes errors to the error log file
    /// </summary>
    public class ErrorLogger : FileLoggerBase
    {
        public ErrorLogger() : base(Globals.ErrorLogPath)
        {
        }
    }
}