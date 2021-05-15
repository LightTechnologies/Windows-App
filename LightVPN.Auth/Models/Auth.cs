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
using System.Text.Json.Serialization;

namespace LightVPN.Auth.Models
{
    /// <summary>
    /// The class used to serialize the authentication data file
    /// </summary>
    public class AuthFile
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}