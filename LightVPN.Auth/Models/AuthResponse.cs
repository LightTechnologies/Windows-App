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

using System;
using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }
    }
}