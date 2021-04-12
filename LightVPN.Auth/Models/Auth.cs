/* --------------------------------------------
 * 
 * Authentication file - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: AuthFile.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */

using System;

namespace LightVPN.Auth.Models
{
    public struct AuthFile
    {
        public string Username { get; set; }
        public Guid SessionId { get; set; }

    }
}
