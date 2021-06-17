using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Client.Windows.Services.Interfaces
{
    public interface IOpenVpnService
    {
        /// <summary>
        /// Locates a OpenVPN configuration file in the cache and tells the OpenVPN manager to connect to it.
        /// </summary>
        /// <param name="serverName">Server name (normally the Pritunl name)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task ConnectAsync(string serverName, CancellationToken cancellationToken = default);
    }
}