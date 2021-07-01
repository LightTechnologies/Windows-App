using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Client.Windows.Services.Interfaces
{
    public interface ICacheService
    {
        Task CacheOpenVpnBinariesAsync(bool force = false, CancellationToken cancellationToken = default);
        Task CacheOpenVpnDriversAsync(bool force = false, CancellationToken cancellationToken = default);
        Task CacheServersAsync(bool force = false, CancellationToken cancellationToken = default);
    }
}
