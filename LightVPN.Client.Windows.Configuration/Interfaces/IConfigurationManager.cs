using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Client.Windows.Configuration.Interfaces
{
    public interface IConfigurationManager
    {
        Task WriteAsync<T>(T value, CancellationToken cancellationToken = default);
        void Write<T>(T value);
        Task<T> ReadAsync<T>(CancellationToken cancellationToken = default);
        T Read<T>();
    }
}