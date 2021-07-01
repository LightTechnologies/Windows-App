using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Models;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Services.Interfaces;
using LightVPN.Client.Windows.Services.Models;

namespace LightVPN.Client.Windows.Services
{
    /// <inheritdoc />
    public class CacheService : ICacheService
    {
        public async Task CacheOpenVpnBinariesAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(Globals.OpenVpnPath) ||
                !File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")) || force)
            {
                var apiClient = Globals.Container.GetInstance<IApiClient>();

                if (Directory.Exists(Globals.OpenVpnPath)) Directory.Delete(Globals.OpenVpnPath, true);

                Directory.CreateDirectory(Globals.OpenVpnPath);

                var ovpnArchive = Path.Combine(Globals.OpenVpnPath, "tmp.zip");
                var ovpnResponse = await apiClient.GetAsync<byte[]>("download/ovpn", cancellationToken);

                await File.WriteAllBytesAsync(ovpnArchive, ovpnResponse, cancellationToken);
                ZipFile.ExtractToDirectory(ovpnArchive, Globals.OpenVpnPath);
                File.Delete(ovpnArchive);
            }
        }

        private static void VerifyCacheIntegrity()
        {
            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppCachePath);
            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppOpenVpnCachePath);
            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppServerCachePath);
        }

        public async Task CacheApiServerResponseAsync(BindingList<DisplayVpnServer> servers, bool force = false,
            CancellationToken cancellationToken = default)
        {
            VerifyCacheIntegrity();

            var fileLocation = Path.Combine(Globals.AppServerCachePath, "cache.json"));

            if (File.Exists(fileLocation) && !force)
            {
                var serverCache =
                    JsonSerializer.Deserialize<ServerCache>(await File.ReadAllTextAsync(fileLocation, cancellationToken));

                if (DateTime.Now < serverCache.LastCache.AddHours(12)) return;
            }

            var o = new ServerCache()
            {
                Servers = servers,
                LastCache = DateTime.Now
            };

            var json = JsonSerializer.Serialize(o);
            await File.WriteAllTextAsync(fileLocation, json, cancellationToken);
        }

        public async Task CacheOpenVpnDriversAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(Globals.OpenVpnDriversPath) || force)
            {
                var apiClient = Globals.Container.GetInstance<IApiClient>();

                if (Directory.Exists(Globals.OpenVpnDriversPath)) Directory.Delete(Globals.OpenVpnDriversPath, true);

                Directory.CreateDirectory(Globals.OpenVpnDriversPath);

                var driverArchive = Path.Combine(Globals.OpenVpnDriversPath, "tmp.zip");
                var driverResponse = await apiClient.GetAsync<byte[]>("download/drivers", cancellationToken);

                await File.WriteAllBytesAsync(driverArchive, driverResponse, cancellationToken);
                ZipFile.ExtractToDirectory(driverArchive, Globals.OpenVpnDriversPath);
                File.Delete(driverArchive);
            }
        }

        public async Task CacheServersAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(Globals.AppCachePath) || force)
            {
                var apiClient = Globals.Container.GetInstance<IApiClient>();

                if (Directory.Exists(Globals.AppCachePath)) Directory.Delete(Globals.AppCachePath, true);

                Directory.CreateDirectory(Globals.AppCachePath);

                var archive = Path.Combine(Globals.AppCachePath, "tmp.zip");
                var configResponse = await apiClient.GetAsync<VpnConfigResponse>("configs", cancellationToken);

                await File.WriteAllBytesAsync(archive, Convert.FromBase64String(configResponse.ArchiveBase64),
                    cancellationToken);
                ZipFile.ExtractToDirectory(archive, Globals.AppCachePath);

                foreach (var file in Directory.GetFiles(Globals.AppCachePath))
                {
                    var lines = await File.ReadAllLinesAsync(file, cancellationToken);

                    var newLines = lines.Where(line => !line.Contains("udp6") && !line.StartsWith('#')).ToList();

                    await File.WriteAllLinesAsync(file, newLines, cancellationToken);
                }

                File.Delete(archive);
            }
        }
    }
}
