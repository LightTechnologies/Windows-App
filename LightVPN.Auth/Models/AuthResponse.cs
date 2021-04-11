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
        [JsonProperty("code")] 
        public int Code { get; set; }
        [JsonProperty("sessionid")] 
        public Guid Session { get; set; }
    }
}
