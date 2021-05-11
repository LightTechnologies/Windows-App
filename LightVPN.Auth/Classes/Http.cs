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
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Auth
{
    public class Http : IHttp
    {
        private static DateTime _lastRetrieved = default;

        private static List<Server> _servers = new();

        private readonly ApiHttpClient _apiclient;

        private readonly FileLoggerBase _logger = new ErrorLogger();

        /// <summary>
        /// Initalizes the class
        /// </summary>
        /// <param name="client">The instance of HttpClient the class will use</param>
        public Http(ApiHttpClient checkingClient)
        {
            _apiclient = checkingClient;
            checkingClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version", $"Windows {Assembly.GetEntryAssembly().GetName().Version}");
            _logger.Write("(Http/ctor) Set request headers");
        }

        public static bool HasOpenVpn()
        {
            return Directory.Exists(Globals.OpenVpnPath) || File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe"));
        }

        public static bool IsConfigsCached()
        {
            return Directory.Exists(Globals.ConfigPath) && Directory.EnumerateFiles(Globals.ConfigPath).Any();
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
            if (!Directory.Exists(Globals.ConfigPath) || force)
            {
                _logger.Write("(Http/CacheConfigs) Caching user server config files");
                if (Directory.Exists(Globals.ConfigPath))
                {
                    Directory.Delete(Globals.ConfigPath, true);
                    Directory.CreateDirectory(Globals.ConfigPath);
                }
                else
                {
                    Directory.CreateDirectory(Globals.ConfigPath);
                }
                var vpnzip = Path.Combine(Globals.ConfigPath, "configs.zip");
                var resp = await _apiclient.GetAsync<ConfigResponse>("https://lightvpn.org/api/configs", cancellationToken);

                _logger.Write("(Http/CacheConfigs) Writing cache");
                await File.WriteAllBytesAsync(vpnzip, Convert.FromBase64String(resp.ConfigArchiveBase64), cancellationToken);
                ZipFile.ExtractToDirectory(vpnzip, Globals.ConfigPath);
                foreach (var file in Directory.GetFiles(Globals.ConfigPath))
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

        public async Task FetchOpenVpnDriversAsync(CancellationToken cancellationToken = default)
        {
            _logger.Write("(Http/FetchOpenVpnDrivers) Downloading the TAP drivers");
            if (Directory.Exists(Globals.OpenVpnDriversPath))
            {
                Directory.Delete(Globals.OpenVpnDriversPath, true);
            }
            Directory.CreateDirectory(Globals.OpenVpnDriversPath);
            var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/download/drivers", cancellationToken);

            await _apiclient.CheckResponseAsync(resp, cancellationToken);

            _logger.Write("(Http/FetchOpenVpnDrivers) Installing the TAP drivers");
            await File.WriteAllBytesAsync(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), await resp.Content.ReadAsByteArrayAsync(cancellationToken), cancellationToken);
            ZipFile.ExtractToDirectory(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), Globals.OpenVpnDriversPath);
            File.Delete(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"));
        }

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
                if (!Directory.Exists(Globals.OpenVpnPath) || !File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")))
                {
                    _logger.Write("(Http/GetOpenVpnBinaries) Downloading OpenVPN binaries");
                    var vpnzip = Path.Combine(Globals.ConfigPath, "openvpn.zip");
                    Directory.CreateDirectory(Globals.OpenVpnPath);
                    var resp = await _apiclient.GetByteArrayAsync($"https://lightvpn.org/api/download/ovpn", cancellationToken);

                    await File.WriteAllBytesAsync(vpnzip, resp, cancellationToken);
                    ZipFile.ExtractToDirectory(vpnzip, Globals.OpenVpnPath);
                    File.Delete(vpnzip);
                    return true;
                }
                else
                {
                    _logger.Write("(Http/GetOpenVpnBinaries) Binaries are already existant, continue");
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
        public async Task<List<Server>> GetServersAsync(CancellationToken cancellationToken = default)
        {
            // This is memory based caching... but Toshi kind of didn't set it up correctly, so I
            // fixed it
            if (DateTime.Now < _lastRetrieved.AddHours(1))
            {
                _logger.Write("(Http/GetServers) Cache renew time hasn't passed, returning cached servers");
                return _servers;
            }

            _servers = await _apiclient.GetAsync<List<Server>>("https://lightvpn.org/api/servers", cancellationToken);

            _lastRetrieved = DateTime.Now;
            _logger.Write("(Http/GetServers) Cached servers in memory");
            return _servers;
        }

        public async Task GetUpdatesAsync(CancellationToken cancellationToken = default)
        {
            _logger.Write("(Http/GetUpdates) Downloading the updater");
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
            _logger.Write("(Http/Login) Authenticating user");
            var model = new { username, password };
            var response = await _apiclient.PostAsync<AuthResponse>("https://lightvpn.org/api/auth", model, cancellationToken);
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {response.SessionId}");
            return response;
        }

        public async Task<bool> ValidateSessionAsync(string username, Guid sessionId, CancellationToken cancellationToken = default)
        {
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {sessionId}");

            var resp = await _apiclient.GetAsync("https://lightvpn.org/api/profile", cancellationToken);
            await _apiclient.CheckResponseAsync(resp, cancellationToken);

            if (resp.IsSuccessStatusCode)
            {
                _logger.Write("(Http/ValidateSession) Session seems to be valid");
                return true;
            }
            else
            {
                _logger.Write("(Http/ValidateSession) Session was invalid, removing references");
                _apiclient.DefaultRequestHeaders.Remove("Authorization");
                return false;
            }
        }
    }
}