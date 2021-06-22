using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.OpenVPN.Exceptions;

namespace LightVPN.Client.Windows.Services.Interfaces
{
    /// <summary>
    ///     Interface for the OpenVpnService
    /// </summary>
    public interface IOpenVpnService
    {
        /// <summary>
        ///     Locates a OpenVPN configuration file in the cache and tells the OpenVPN manager to connect to it.
        /// </summary>
        /// <param name="location">Server location</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="id">Server ID</param>
        /// <exception cref="T:System.InvalidOperationException">
        ///     Thrown when attempting to connect whilst connected or connecting
        ///     or if the configuration file doesn't exist
        /// </exception>
        /// <exception cref="AuthenticationException">Thrown when the authentication process fails</exception>
        /// <exception cref="FileLoadException">Thrown when OpenVPN rejects a configuration file for whatever reason</exception>
        /// <exception cref="UnknownErrorException">Thrown when OpenVPN spits out Unknown error into stdout</exception>
        /// <exception cref="TimeoutException">Thrown when the connection to a VPN server times out</exception>
        /// <returns></returns>
        Task ConnectAsync(string id, string location, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Caches VPN server files
        /// </summary>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CacheServersAsync(bool force = false, CancellationToken cancellationToken = default);
    }
}