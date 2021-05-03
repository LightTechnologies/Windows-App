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
using LightVPN.Auth.Classes;
using LightVPN.Auth.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.Models;
using LightVPN.Logger;
using LightVPN.Logger.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static LightVPN.Auth.ApiException;

namespace LightVPN.Auth
{
    public class Http : IHttp
    {
        private readonly HttpClient _apiclient = null;
        private readonly FileLogger _logger = new ErrorLogger();
        private static DateTime _lastRetrieved = default;
        private static List<Server> _servers = new();
        /// <summary>
        /// Initalizes the class
        /// </summary>
        /// <param name="client">The instance of HttpClient the class will use</param>
        public Http(SSLCheckingHttpClient checkingClient)
        {
            _apiclient = checkingClient;
            checkingClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version", $"Windows {Assembly.GetEntryAssembly().GetName().Version}");
            _logger.Write("(Http/ctor) Set request headers");
        }

        [Obsolete("This is no longer needed due to automated version checks on the API endpoints", true)]
        public async Task<string> GetVersionAsync()
        {
            try
            {
                return await _apiclient.GetStringAsync("https://lightvpn.org/api/client/version");
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> ValidateSessionAsync(string username, Guid sessionId)
        {
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {sessionId}");
            
            var resp = await _apiclient.GetAsync("https://lightvpn.org/api/profile");
            await CheckResponseAsync(resp);
            
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
        public async Task GetUpdatesAsync()
        {
            _logger.Write("(Http/GetUpdates) Downloading the updater");
            var updaterPath = Path.Combine(Path.GetTempPath(), "LightVPNUpdater.exe");
            var fileBytes = await _apiclient.GetByteArrayAsync("https://lightvpn.org/api/download/updater");
            await File.WriteAllBytesAsync(updaterPath, fileBytes);
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
        public async Task FetchOpenVpnDriversAsync()
        {
            _logger.Write("(Http/FetchOpenVpnDrivers) Downloading the TAP drivers");
            if (Directory.Exists(Globals.OpenVpnDriversPath))
            {
                Directory.Delete(Globals.OpenVpnDriversPath, true);
            }
            Directory.CreateDirectory(Globals.OpenVpnDriversPath);
            var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/download/drivers");

            await CheckResponseAsync(resp);
            
            _logger.Write("(Http/FetchOpenVpnDrivers) Installing the TAP drivers");
            await File.WriteAllBytesAsync(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), await resp.Content.ReadAsByteArrayAsync());
            ZipFile.ExtractToDirectory(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), Globals.OpenVpnDriversPath);
            File.Delete(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"));
        }

        public async Task<string> GetChangelogAsync()
        {
            var changelog = await GetAsync<ChangelogModel>("https://lightvpn.org/api/changelog?platform=windows");

            return changelog.Changelog;
        }

        /// <summary>
        /// Fetches the servers asynchronously from the LightVPN API
        /// </summary>
        /// <returns>List of servers in a enumerable list of server objects</returns>
        public async Task<List<Server>> GetServersAsync()
        {
            // This is memory based caching... but Toshi kind of didn't set it up correctly, so I fixed it
            if (DateTime.Now < _lastRetrieved.AddHours(1))
            {
                _logger.Write("(Http/GetServers) Cache renew time hasn't passed, returning cached servers");
                return _servers;
            }

            _servers = await GetAsync<List<Server>>("https://lightvpn.org/api/servers");

            _lastRetrieved = DateTime.Now;
            _logger.Write("(Http/GetServers) Cached servers in memory");
            return _servers;
        }
        /// <summary>
        /// Fetches the OpenVPN binaries required for operation of connecting to servers
        /// </summary>
        /// <returns>True or false whether it successfully was able to fetch and cache the binaries</returns>
        public async Task<bool> GetOpenVpnBinariesAsync()
        {
            try
            {
                if (!Directory.Exists(Globals.OpenVpnPath) || !File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")))
                {
                    _logger.Write("(Http/GetOpenVpnBinaries) Downloading OpenVPN binaries");
                    var vpnzip = Path.Combine(Globals.ConfigPath, "openvpn.zip");
                    Directory.CreateDirectory(Globals.OpenVpnPath);
                    var resp = await _apiclient.GetByteArrayAsync($"https://lightvpn.org/api/download/ovpn");

                    await File.WriteAllBytesAsync(vpnzip, resp);
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
        public Task<bool> HasOpenVpnAsync()
        {
            return Task.FromResult(Directory.Exists(Globals.OpenVpnPath) || File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")));
        }
        /// <summary>
        /// Fetches the VPN server configurations asynchronously with the specified users API credentials, returned after authentication is successful
        /// </summary>
        /// <param name="force">Makes the method ignore whether they are already cached, and just cache them again</param>
        /// <returns></returns>
        public async Task CacheConfigsAsync(bool force = false)
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
                var resp = await GetAsync<ConfigResponse>("https://lightvpn.org/api/configs");

                _logger.Write("(Http/CacheConfigs) Writing cache");
                await File.WriteAllBytesAsync(vpnzip, Convert.FromBase64String(resp.ConfigArchiveBase64));
                ZipFile.ExtractToDirectory(vpnzip, Globals.ConfigPath);
                foreach (var file in Directory.GetFiles(Globals.ConfigPath))
                {
                    var newfile = new List<string>();
                    var lines = await File.ReadAllLinesAsync(file);
                    foreach (var line in lines)
                    {
                        if (line.Contains("udp6")) continue;
                        newfile.Add(line);
                    }
                    await File.WriteAllLinesAsync(file, newfile);
                }
                File.Delete(vpnzip);
            }
        }
        public Task<bool> IsConfigsCachedAsync()
        {
            return Task.FromResult<bool>(Directory.Exists(Globals.ConfigPath) && Directory.EnumerateFiles(Globals.ConfigPath).Any());
        }
        /// <summary>
        /// Authenticates the user with the specified username and password, and returns the info we need, the API credentials. Throws the API exceptions if something messes up
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>AuthResponse object which can be null</returns>
        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            _logger.Write("(Http/Login) Authenticating user");
            var model = new { username, password };
            var response = await PostAsync<AuthResponse>("https://lightvpn.org/api/auth", model);
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {response.Session}");
            return response;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            _logger.Write("(Http/Get) Getting data");
            var resp = await _apiclient.GetAsync(url);
            await CheckResponseAsync(resp);
            return JsonConvert.DeserializeObject<T>(await resp.Content.ReadAsStringAsync());
        }

        public async Task<T> PostAsync<T>(string url, object body)
        {
            _logger.Write("(Http/Post) Posting JSON data");
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var resp = await _apiclient.PostAsync(url, content);
            await CheckResponseAsync(resp);
            return JsonConvert.DeserializeObject<T>(await resp.Content.ReadAsStringAsync());
        }

        private static async Task CheckResponseAsync(HttpResponseMessage resp)
        {
            var content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode == HttpStatusCode.Forbidden || resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.BadRequest )
            {
                try
                {
                    var errorresp = JsonConvert.DeserializeObject<GenericResponse>(content);

                    throw new InvalidResponseException(errorresp.Message);
                }
                catch (JsonException)
                {
                    throw new InvalidResponseException("You are being ratelimited");
                }
            }
            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RatelimitedException("You are being ratelimited");
            }
            if(resp.StatusCode == HttpStatusCode.UpgradeRequired)
            {
                throw new ClientUpdateRequired("A client update is required");
            }
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.");
            }
        }
    }
}
