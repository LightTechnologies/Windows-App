using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Services.Interfaces;

namespace LightVPN.Client.Windows.Services
{
    public sealed class OpenVpnService : IOpenVpnService
    {
        private readonly IVpnManager _vpnManager;

        public OpenVpnService(IVpnManager vpnManager)
        {
            _vpnManager = vpnManager;
        }

        /// <inheritdoc />
        /// <summary>
        /// Locates a OpenVPN configuration file in the cache and tells the OpenVPN manager to connect to it.
        /// </summary>
        /// <param name="serverName">Server name (normally the Pritunl name)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task ConnectAsync(string serverName, CancellationToken cancellationToken = default)
        {
            var files = Directory.GetFiles(Globals.AppCachePath);

            if (files.Length == 0 || !files.Any(x => x.Contains(serverName)))
            {
                // Config not found! This here makes the OpenVPN manager's check a race condition.
#if DEBUG
                Debug.WriteLine("Configuration file not found in cache, this is preventing a race condition.");
#endif
                return;
            }

            var configFileName = files.FirstOrDefault(x => x.Contains(serverName));
            if (string.IsNullOrWhiteSpace(configFileName))
            {
#if DEBUG
                Debug.WriteLine("Configuration filename is null/ws this is really odd.");
#endif

                return;
            }

            await _vpnManager.ConnectAsync(configFileName, cancellationToken);
        }
    }
}