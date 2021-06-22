using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.OpenVPN.Exceptions;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Services.Interfaces;

namespace LightVPN.Client.Windows.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     Essentially a Windows only wrapper for the OpenVPN manager class
    /// </summary>
    public sealed partial class OpenVpnService : IOpenVpnService
    {
        /// <inheritdoc />
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
        public async Task ConnectAsync(string id, string location, CancellationToken cancellationToken = default)
        {
            var vpnManager = Globals.Container.GetInstance<IVpnManager>();

            var files = Directory.GetFiles(Globals.AppCachePath);

            if (files.Length == 0 || !files.Any(x => x.Contains(id)))
            {
                // Config not found! This here makes the OpenVPN manager's check a race condition.
#if DEBUG
                Debug.WriteLine("Configuration file not found in cache, this is preventing a race condition.");
#endif
                return;
            }

            var configFileName = files.FirstOrDefault(x => x.Contains(id));
            if (string.IsNullOrWhiteSpace(configFileName))
            {
#if DEBUG
                Debug.WriteLine("Configuration filename is null/ws this is really odd.");
#endif

                return;
            }

            var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
            var currentConfig = await manager.ReadAsync(cancellationToken);

            currentConfig.LastServer = new AppLastServer
            {
                Location = location,
                PritunlName = id
            };

            await manager.WriteAsync(currentConfig, cancellationToken);

            await vpnManager.ConnectAsync(configFileName, cancellationToken);
        }
    }
}