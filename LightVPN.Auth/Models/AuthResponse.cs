/* --------------------------------------------
 *
 * Authentication response - Model
 * Copyright (C) Light Technologies LLC
 *
 * File: AuthResponse.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using Newtonsoft.Json;
using System;

namespace LightVPN.Auth.Models
{
    public struct AuthResponse
    {
        public int Code { get; set; }

        public Guid SessionId { get; set; }
    }
}