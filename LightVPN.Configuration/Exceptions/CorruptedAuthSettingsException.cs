/* --------------------------------------------
 * 
 * API exceptions - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: ApiException.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using System;

namespace LightVPN.Settings.Exceptions
{
    public class CorruptedAuthSettingsException : Exception
    {
        public CorruptedAuthSettingsException()
        {
        }

        public CorruptedAuthSettingsException(string message)
            : base(message)
        {
        }

        public CorruptedAuthSettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
