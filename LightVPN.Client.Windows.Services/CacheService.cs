namespace LightVPN.Client.Windows.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Auth.Interfaces;
    using Auth.Models;
    using Common;
    using Common.Models;
    using Common.Utils;
    using Cryptography;
    using Debug;
    using Interfaces;
    using Models;

    /// <inheritdoc />
    public sealed class CacheService : ICacheService
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
                return;
            }

            DebugLogger.Write("lvpn-client-services-cacheman",
                "cache call has been ignored for ovpn binaries");
        }

        private static void VerifyCacheIntegrity()
        {
            DebugLogger.Write("lvpn-client-services-cacheman",
                "verifying cache integrity");

            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppCachePath);
            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppOpenVpnCachePath);
            DirectoryUtils.DirectoryNotExistsCreate(Globals.AppServerCachePath);
        }

        public async Task<BindingList<DisplayVpnServer>> GetCachedApiServerResponseAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                CacheService.VerifyCacheIntegrity();

                var fileLocation = Path.Combine(Globals.AppServerCachePath, "cache.json");

                if (!File.Exists(fileLocation)) return null;

                var serverCache =
                    JsonSerializer.Deserialize<ServerCache>(
                        await File.ReadAllTextAsync(fileLocation, cancellationToken));

                // Cache has expired, return null to get the ViewModel to re-cache
                return DateTime.Now < serverCache?.LastCache.AddHours(12) ? serverCache.Servers : null;
            }
            catch (Exception e)
            {
                DebugLogger.Write("lvpn-client-services-cacheman", $"api fetch from file cache exception: {e}");
                return null;
            }
        }

        public async Task CacheApiServerResponseAsync(BindingList<DisplayVpnServer> servers, bool force = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                CacheService.VerifyCacheIntegrity();

                var fileLocation = Path.Combine(Globals.AppServerCachePath, "cache.json");

                if (File.Exists(fileLocation) && !force)
                {
                    DebugLogger.Write("lvpn-client-services-cacheman",
                        "checking last server cache time");

                    var serverCache =
                        JsonSerializer.Deserialize<ServerCache>(
                            await File.ReadAllTextAsync(fileLocation, cancellationToken));

                    if (DateTime.Now < serverCache?.LastCache.AddHours(12)) return;
                }

                DebugLogger.Write("lvpn-client-services-cacheman",
                    "caching new list of servers");

                var o = new ServerCache
                {
                    Servers = servers,
                    LastCache = DateTime.Now,
                };

                var json = JsonSerializer.Serialize(o);
                await File.WriteAllTextAsync(fileLocation, json, cancellationToken);
            }
            catch (Exception e)
            {
                DebugLogger.Write("lvpn-client-services-cacheman", $"api write cache exception: {e}");
            }
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

                return;
            }

            DebugLogger.Write("lvpn-client-services-cacheman",
                "cache call has been ignored for ovpn drivers");
        }

        public async Task CacheServersAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(Globals.AppOpenVpnCachePath) || force)
            {
                var apiClient = Globals.Container.GetInstance<IApiClient>();

                CacheService.VerifyCacheIntegrity();

                if (Directory.Exists(Globals.AppOpenVpnCachePath)) Directory.Delete(Globals.AppOpenVpnCachePath, true);

                Directory.CreateDirectory(Globals.AppOpenVpnCachePath);

                var archive = Path.Combine(Globals.AppOpenVpnCachePath, "tmp.zip");
                var configResponse = await apiClient.GetAsync<VpnConfigResponse>("configs", cancellationToken);

                await File.WriteAllBytesAsync(archive, Convert.FromBase64String(configResponse.ArchiveBase64),
                    cancellationToken);
                ZipFile.ExtractToDirectory(archive, Globals.AppOpenVpnCachePath);

                foreach (var file in Directory.GetFiles(Globals.AppOpenVpnCachePath))
                {
                    // Here we're going to tweak the configuration files a little bit to reduce size and increase OpenVPNs speed.
                    var lines = await File.ReadAllLinesAsync(file, cancellationToken);

                    // Make sure that no comments or anything containing udp6 or max-routes are included, this should increase speed and decrease config size
                    var tmpLines = lines.Where(line =>
                        !line.Contains("udp6") && !line.StartsWith('#') && !line.StartsWith("max-routes")).ToList();

                    // Renames the ciphers directive to data-ciphers, the tradeoff is incompatibility with OpenVPN versions below 2.5 but increases speed and performance.
                    tmpLines = tmpLines.Select(newLine => newLine.Replace("cipher", "data-ciphers")).ToList();

                    var newLines = new List<string>();

                    // Adds the cipher fallback to whatever is specified in the configuration
                    // We're not hard coding the cipher in here as that will break compatibility if we
                    // Choose to change cipher suite. (as of writing it's AES-128-CBC)
                    foreach (var line in tmpLines)
                    {
                        var array = line.Split(' ');
                        if (array.First() == "data-ciphers") newLines.Add($"data-ciphers-fallback {array.Last()}");

                        newLines.Add(line);
                    }

                    // Writes the updated configuration.
                    await File.WriteAllLinesAsync(file, newLines, cancellationToken);
                }

                File.Delete(archive);

                return;
            }

            DebugLogger.Write("lvpn-client-services-cacheman",
                "cache call has been ignored for ovpn configs");
        }
    }
}
