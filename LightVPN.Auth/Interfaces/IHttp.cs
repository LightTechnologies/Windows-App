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
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Auth.Interfaces
{
    public interface IHttp
    {
        Task CacheConfigsAsync(bool force = false, CancellationToken cancellationToken = default);

        Task FetchOpenVpnDriversAsync(CancellationToken cancellationToken = default);

        Task<bool> GetOpenVpnBinariesAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<Server>> GetServersAsync(CancellationToken cancellationToken = default);

        Task GetUpdatesAsync(CancellationToken cancellationToken = default);

        Task<AuthResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

        Task<bool> ValidateSessionAsync(string username, Guid guid, CancellationToken cancellationToken = default);
    }
}