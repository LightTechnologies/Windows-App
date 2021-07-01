using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Windows.Common.Models;

namespace LightVPN.Client.Windows.Services.Interfaces
{
    public interface ICacheService
    {
        Task CacheOpenVpnBinariesAsync(bool force = false, CancellationToken cancellationToken = default);
        Task CacheOpenVpnDriversAsync(bool force = false, CancellationToken cancellationToken = default);

        Task CacheApiServerResponseAsync(BindingList<DisplayVpnServer> servers, bool force = false,
            CancellationToken cancellationToken = default);
        Task CacheServersAsync(bool force = false, CancellationToken cancellationToken = default);

        Task<BindingList<DisplayVpnServer>> GetCachedApiServerResponseAsync(
            CancellationToken cancellationToken = default);
    }
}
