using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Client.Auth.Interfaces
{
    /// <summary>
    /// Interface for the HttpClient implementation
    /// </summary>
    public interface IApiClient
    {
        Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default);
        Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken = default);
    }
}