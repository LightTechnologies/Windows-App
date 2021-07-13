namespace LightVPN.Client.Windows.Configuration
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Debug;
    using Interfaces;

    public sealed class ConfigurationManager<T> : IConfigurationManager<T>
    {
        private readonly string _configurationPath;

        public ConfigurationManager(string configurationPath)
        {
            this._configurationPath = configurationPath;
        }

        private void Verify()
        {
            /*
             * Check if the configuration file happens to be from v2.x
             * We do this by checking if the configuration file has the "IsFirstRun" property.
             * If not then we know it's an old or corrupt configuration file, as that property is required
             */
            if (File.Exists(this._configurationPath) &&
                !File.ReadAllText(this._configurationPath).Contains("\"IsFirstRun\":"))
            {
                // The next line will detect the missing configuration and re-create it
                DebugLogger.Write("lvpn-client-win-configman",
                    "kissing goodbye to detected lvpn v2.x config file");

                File.Delete(this._configurationPath);
            }

            if (!File.Exists(this._configurationPath)) File.WriteAllText(this._configurationPath, "{}");
        }

        public async Task WriteAsync(T value, CancellationToken cancellationToken = default)
        {
            this.Verify();

            var json = JsonSerializer.Serialize(value);
            await File.WriteAllTextAsync(this._configurationPath, json, cancellationToken);
        }

        public void Write(T value)
        {
            this.Verify();

            var json = JsonSerializer.Serialize(value);
            File.WriteAllText(this._configurationPath, json);
        }

        public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            this.Verify();

            var fileContents = await File.ReadAllTextAsync(this._configurationPath, cancellationToken);
            return JsonSerializer.Deserialize<T>(fileContents);
        }

        public T Read()
        {
            this.Verify();

            var fileContents = File.ReadAllText(this._configurationPath);
            return JsonSerializer.Deserialize<T>(fileContents);
        }
    }
}
