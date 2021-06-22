using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Services.Interfaces;

namespace LightVPN.Client.Windows.Services
{
    public partial class OpenVpnService : IOpenVpnService
    {
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