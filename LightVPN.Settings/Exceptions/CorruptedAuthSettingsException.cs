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
    /// <summary>
    /// Thrown when the encryption class fails to decrypt data passed to it
    /// </summary>
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
