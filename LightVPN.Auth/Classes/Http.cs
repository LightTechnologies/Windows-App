/* --------------------------------------------
 *
 * LightVPN API abstraction layer - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: Http.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.Auth.Exceptions;
using LightVPN.Auth.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Auth
{
    public class Http : IHttp
    {
        private static DateTime _lastRetrieved = default;

        private static HashSet<Server> _servers = new();

        private readonly ApiHttpClient _apiclient;

        private static PlatformID _platform;

        private readonly string ConfigPath;

        private readonly string OpenVpnPath;

        private readonly string OpenVpnDriversPath;

        /// <summary>
        /// Initalizes the class
        /// </summary>
        /// <param name="client">The instance of HttpClient the class will use</param>
        public Http(ApiHttpClient checkingClient, PlatformID platformID)
        {
            _apiclient = checkingClient;
            _platform = platformID;
            if (platformID == PlatformID.Unix)
            {
                checkingClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version", $"Linux {Assembly.GetEntryAssembly().GetName().Version}");
                ConfigPath = Globals.LinuxConfigPath;
                OpenVpnPath = Globals.LinuxOpenVpnPath;
                OpenVpnDriversPath = Globals.LinuxOpenVpnDriversPath;
            }
            else if (platformID == PlatformID.Win32NT)
            {
                checkingClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version", $"Windows {Assembly.GetEntryAssembly().GetName().Version}");
                ConfigPath = Globals.ConfigPath;
                OpenVpnDriversPath = Globals.OpenVpnDriversPath;
                OpenVpnPath = Globals.OpenVpnPath;
            }
        }

        /// <summary>
        /// Checks if the OpenVPN binaries are downloaded on the system
        /// </summary>
        /// <returns>True if they are, false otherwise</returns>
        public static bool HasOpenVpn()
        {
            return Directory.Exists(_platform == PlatformID.Unix ? Globals.LinuxOpenVpnPath : Globals.OpenVpnPath) || File.Exists(Path.Combine(_platform == PlatformID.Unix ? Globals.LinuxOpenVpnPath : Globals.OpenVpnPath, "openvpn.exe"));
        }

        /// <summary>
        /// Checks if the configs are cached on the system
        /// </summary>
        /// <returns>True if they are, false otherwise</returns>
        public static bool IsConfigsCached()
        {
            return Directory.Exists(_platform == PlatformID.Unix ? Globals.LinuxConfigPath : Globals.ConfigPath) && Directory.EnumerateFiles(_platform == PlatformID.Unix ? Globals.LinuxConfigPath : Globals.ConfigPath).Any();
        }

        /// <summary>
        /// Fetches the VPN server configurations asynchronously with the specified users API
        /// credentials, returned after authentication is successful
        /// </summary>
        /// <param name="force">
        /// Makes the method ignore whether they are already cached, and just cache them again
        /// </param>
        /// <returns></returns>
        public async Task CacheConfigsAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(ConfigPath) || force)
            {
                if (Directory.Exists(ConfigPath))
                {
                    Directory.Delete(ConfigPath, true);
                    Directory.CreateDirectory(ConfigPath);
                }
                else
                {
                    Directory.CreateDirectory(ConfigPath);
                }
                var vpnzip = Path.Combine(ConfigPath, "configs.zip");
                var resp = await _apiclient.GetAsync<ConfigResponse>("https://lightvpn.org/api/configs", cancellationToken);

                await File.WriteAllBytesAsync(vpnzip, Convert.FromBase64String(resp.ConfigArchiveBase64), cancellationToken);
                ZipFile.ExtractToDirectory(vpnzip, ConfigPath);
                foreach (var file in Directory.GetFiles(ConfigPath))
                {
                    var newfile = new List<string>();
                    var lines = await File.ReadAllLinesAsync(file, cancellationToken);
                    foreach (var line in lines)
                    {
                        if (line.Contains("udp6")) continue;
                        newfile.Add(line);
                    }
                    await File.WriteAllLinesAsync(file, newfile, cancellationToken);
                }
                File.Delete(vpnzip);
            }
        }

        /// <summary>
        /// Gets and extracts the OpenVPN TAP drivers
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task FetchOpenVpnDriversAsync(CancellationToken cancellationToken = default)
        {
            if (Directory.Exists(OpenVpnDriversPath))
            {
                Directory.Delete(OpenVpnDriversPath, true);
            }
            Directory.CreateDirectory(OpenVpnDriversPath);
            var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/download/drivers", cancellationToken);

            await _apiclient.CheckResponseAsync(resp, cancellationToken);

            await File.WriteAllBytesAsync(Path.Combine(OpenVpnDriversPath, "drivers.zip"), await resp.Content.ReadAsByteArrayAsync(cancellationToken), cancellationToken);
            ZipFile.ExtractToDirectory(Path.Combine(OpenVpnDriversPath, "drivers.zip"), OpenVpnDriversPath);
            File.Delete(Path.Combine(OpenVpnDriversPath, "drivers.zip"));
        }

        /// <summary>
        /// Gets and returns the changelog for the Windows client
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetChangelogAsync(CancellationToken cancellationToken = default)
        {
            var changelog = await _apiclient.GetAsync<Changelog>("https://lightvpn.org/api/changelog?platform=windows", cancellationToken);

            return changelog.Content;
        }

        /// <summary>
        /// Fetches the OpenVPN binaries required for operation of connecting to servers
        /// </summary>
        /// <returns>True or false whether it successfully was able to fetch and cache the binaries</returns>
        public async Task<bool> GetOpenVpnBinariesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Directory.Exists(OpenVpnPath) || !File.Exists(Path.Combine(OpenVpnPath, "openvpn.exe")))
                {
                    var vpnzip = Path.Combine(ConfigPath, "openvpn.zip");
                    Directory.CreateDirectory(OpenVpnPath);
                    var resp = await _apiclient.GetByteArrayAsync($"https://lightvpn.org/api/download/ovpn", cancellationToken);

                    await File.WriteAllBytesAsync(vpnzip, resp, cancellationToken);
                    ZipFile.ExtractToDirectory(vpnzip, OpenVpnPath);
                    File.Delete(vpnzip);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.", e);
            }
        }

        /// <summary>
        /// Fetches the servers asynchronously from the LightVPN API
        /// </summary>
        /// <returns>List of servers in a enumerable list of server objects</returns>
        public async Task<IEnumerable<Server>> GetServersAsync(CancellationToken cancellationToken = default)
        {
            // This is memory based caching... but Toshi kind of didn't set it up correctly, so I
            // fixed it
            if (DateTime.Now < _lastRetrieved.AddHours(1))
            {
                return _servers;
            }

            _servers = await _apiclient.GetAsync<HashSet<Server>>("https://lightvpn.org/api/servers", cancellationToken);

            _lastRetrieved = DateTime.Now;
            return _servers;
        }

        /// <summary>
        /// Downloads the updater and runs it to install the latest version
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (_platform == PlatformID.Unix) throw new InvalidOperationException("This method is only callable on Windows platforms");

            var updaterPath = Path.Combine(Path.GetTempPath(), "LightVPNUpdater.exe");
            var fileBytes = await _apiclient.GetByteArrayAsync("https://lightvpn.org/api/download/updater", cancellationToken);
            await File.WriteAllBytesAsync(updaterPath, fileBytes, cancellationToken);
            var prc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };

            prc.Start();

            Environment.Exit(0);
        }

        /// <summary>
        /// Authenticates the user with the specified username and password, and returns the info we
        /// need, the API credentials. Throws the API exceptions if something messes up
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>AuthResponse object which can be null</returns>
        public async Task<AuthResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var model = new { username, password };
            var response = await _apiclient.PostAsync<AuthResponse>("https://lightvpn.org/api/auth", model, cancellationToken);
            AssignAuthHeader(username, response.SessionId);
            return response;
        }

        private void AssignAuthHeader(string username, Guid sessionId)
        {
            _apiclient.DefaultRequestHeaders.Remove("Authorization");
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {sessionId}");
        }

        /// <summary>
        /// Validates a session ID
        /// </summary>
        /// <param name="username">Username the session is tied to</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if the session is valid, false otherwise</returns>
        public async Task<bool> ValidateSessionAsync(string username, Guid sessionId, CancellationToken cancellationToken = default)
        {
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {sessionId}");

            var resp = await _apiclient.GetAsync("https://lightvpn.org/api/profile", cancellationToken);
            await _apiclient.CheckResponseAsync(resp, cancellationToken);

            if (resp.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                _apiclient.DefaultRequestHeaders.Remove("Authorization");
                return false;
            }
        }
    }
}