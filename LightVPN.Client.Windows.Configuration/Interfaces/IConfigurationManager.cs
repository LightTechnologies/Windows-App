using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Client.Windows.Configuration.Interfaces
{
    public interface IConfigurationManager<T>
    {
        Task WriteAsync(T value, CancellationToken cancellationToken = default);
        void Write(T value);
        Task<T> ReadAsync(CancellationToken cancellationToken = default);
        T Read();
    }
}