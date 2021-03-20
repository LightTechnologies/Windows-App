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

namespace LightVPN.Auth.Models
{
    public struct AuthResponse
    {
        [JsonProperty("status")] 
        public bool Status { get; set; }
        [JsonProperty("code")] 
        public int Code { get; set; }
        [JsonProperty("error")] 
        public string Error { get; set; }
        [JsonProperty("ovpn_username")] 
        public string OpenVPNUsername { get; set; }
        [JsonProperty("ovpn_password")] 
        public string OpenVPNPassword { get; set; }
        [JsonProperty("email")] 
        public string Email { get; set; }
    }
}
