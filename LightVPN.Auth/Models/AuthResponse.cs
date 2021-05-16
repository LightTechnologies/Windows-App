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
    /// <summary>
    /// Contains data about an authentication request result
    /// </summary>
    public class AuthResponse : GenericResponse
    {

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }
    }
}