/* --------------------------------------------
 * 
 * File logger - Error logger
 * Copyright (C) Light Technologies LLC
 * 
 * File: ErorrLogger.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Common.Models;
using LightVPN.Logger.Base;

namespace LightVPN.Logger
{
    public class ErrorLogger : FileLogger
    {
        public ErrorLogger() : base(Globals.ErrorLogPath)
        {

        }
    }
}
