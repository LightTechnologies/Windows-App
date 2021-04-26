/* --------------------------------------------
 * 
 * LightVPN API abstraction layer - Interface
 * Copyright (C) Light Technologies LLC
 * 
 * File: IHttp.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Auth.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LightVPN.Auth.Interfaces
{
    public interface IHttp
    {
        Task<string> GetVersionAsync();
        Task GetUpdatesAsync();
        Task FetchOpenVpnDriversAsync();
        Task CacheConfigsAsync(bool force = false);
        Task<bool> GetOpenVPNBinariesAsync();
        Task<List<Server>> GetServersAsync();
        Task<AuthResponse> LoginAsync(string username, string password);
        Task<bool> IsConfigsCachedAsync();
        Task<bool> HasOpenVPN();
        Task<bool> ValidateSession(string username, Guid guid);
    }
}