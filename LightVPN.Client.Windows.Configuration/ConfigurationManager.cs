using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Debug;
using LightVPN.Client.Windows.Configuration.Interfaces;

namespace LightVPN.Client.Windows.Configuration
{
    public sealed class ConfigurationManager<T> : IConfigurationManager<T>
    {
        private readonly string _configurationPath;

        public ConfigurationManager(string configurationPath)
        {
            _configurationPath = configurationPath;
        }

        private void Verify()
        {
            /*
             * Check if the configuration file happens to be from v2.x
             * We do this by checking if the configuration file has the "IsFirstRun" property.
             * If not then we know it's an old or corrupt configuration file, as that property is required
             */
            if (File.Exists(_configurationPath) && !File.ReadAllText(_configurationPath).Contains("\"IsFirstRun\":"))
            {
                // The next line will detect the missing configuration and re-create it
                DebugLogger.Write("lvpn-client-win-configman",
                    "kissing goodbye to detected lvpn v2.x config file");

                File.Delete(_configurationPath);
            }

            if (!File.Exists(_configurationPath)) File.WriteAllText(_configurationPath, "{}");
        }

        public async Task WriteAsync(T value, CancellationToken cancellationToken = default)
        {
            Verify();

            var json = JsonSerializer.Serialize(value);
            await File.WriteAllTextAsync(_configurationPath, json, cancellationToken);
        }

        public void Write(T value)
        {
            Verify();

            var json = JsonSerializer.Serialize(value);
            File.WriteAllText(_configurationPath, json);
        }

        public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            Verify();

            var fileContents = await File.ReadAllTextAsync(_configurationPath, cancellationToken);
            return JsonSerializer.Deserialize<T>(fileContents);
        }

        public T Read()
        {
            Verify();

            var fileContents = File.ReadAllText(_configurationPath);
            return JsonSerializer.Deserialize<T>(fileContents);
        }
    }
}
