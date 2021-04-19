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
using LightVPN.Common.v2.Models;
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
        // I wrote this sexy http class (khrysus)
        private readonly HttpClient _apiclient = null;
        private static DateTime _lastRetrieved = DateTime.MinValue;
        private static List<Server> servers;
        /// <summary>
        /// Initalizes the class
        /// </summary>
        /// <param name="client">The instance of HttpClient the class will use</param>
        public Http(SSLCheckingHttpClient checkingClient)
        {
            _apiclient = checkingClient;
            checkingClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version", $"Windows {Assembly.GetEntryAssembly().GetName().Version}");
        }

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
        public async Task<bool> ValidateSession(string username, Guid guid)
        {
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {guid}");
            var resp = await _apiclient.GetAsync("https://lightvpn.org/api/profile");

            if (resp.IsSuccessStatusCode) return true;
            else
            {
                _apiclient.DefaultRequestHeaders.Remove("Authorization");
                return false;
            }
        }
        public async Task GetUpdatesAsync()
        {
            await Task.Delay(1000);// flux ratelimit system is utter aids and needs to be redone entirely because retardation  i dont want to have this here but I have no option ~ Toshi (is gay)
            var updaterpath = Path.Combine(Path.GetTempPath(), "LightVPNUpdater.exe");
            var filebytes = await _apiclient.GetByteArrayAsync("https://lightvpn.org/api/download/updater");
            await File.WriteAllBytesAsync(updaterpath, filebytes);
            var prc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = updaterpath,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };

            prc.Start();

            Environment.Exit(0);
        }
        public async Task FetchOpenVpnDriversAsync()
        {
            await Task.Delay(1000);// flux ratelimit system is utter aids and needs to be redone entirely because retardation  i dont want to have this here but I have no option ~ Toshi
            if (Directory.Exists(Globals.OpenVpnDriversPath))
            {
                Directory.Delete(Globals.OpenVpnDriversPath, true);
            }
            Directory.CreateDirectory(Globals.OpenVpnDriversPath);
            var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/download/drivers");
            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RatelimitedException("You are being ratelimited");
            }
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.", await resp.Content.ReadAsStringAsync(), (int)resp.StatusCode);
            }
            await File.WriteAllBytesAsync(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), await resp.Content.ReadAsByteArrayAsync());
            ZipFile.ExtractToDirectory(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"), Globals.OpenVpnDriversPath);
            File.Delete(Path.Combine(Globals.OpenVpnDriversPath, "drivers.zip"));
        }

        /// <summary>
        /// Fetches the servers asynchronously from the LightVPN API
        /// </summary>
        /// <returns>List of servers in a enumerable list of server objects</returns>
        public async Task<List<Server>> GetServersAsync()
        {
            if (DateTime.Now < _lastRetrieved.AddHours(1)) 
            {
                return servers;
            }
            var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/servers");
            var content = await resp.Content.ReadAsStringAsync();
            await CheckResponse(resp);
            servers = JsonConvert.DeserializeObject<List<Server>>(content);
            _lastRetrieved = DateTime.Now;
            return servers;
        }
        /// <summary>
        /// Fetches the OpenVPN binaries required for operation of connecting to servers
        /// </summary>
        /// <returns>True or false whether it successfully was able to fetch and cache the binaries</returns>
        public async Task<bool> GetOpenVPNBinariesAsync()
        {
            try
            {
                await Task.Delay(1000);// flux ratelimit system is utter aids and needs to be redone entirely because retardation  i dont want to have this here but I have no option ~ Toshi
                if (!Directory.Exists(Globals.OpenVpnPath) || !File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")))
                {
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
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.", e);
            }
        }
        public Task<bool> HasOpenVPN()
        {
            return Task.FromResult<bool>(Directory.Exists(Globals.OpenVpnPath) || File.Exists(Path.Combine(Globals.OpenVpnPath, "openvpn.exe")));
        }
        /// <summary>
        /// Fetches the VPN server configurations asynchronously with the specified users API credentials, returned after authentication is successful
        /// </summary>
        /// <param name="force">Makes the method ignore whether they are already cached, and just cache them again</param>
        /// <returns></returns>
        public async Task CacheConfigsAsync(bool force = false)
        {
            // flux ratelimit system is utter aids and needs to be redone entirely because retardation  i dont want to have this here but I have no option ~ Toshi
            // this function was made because of retarded pritunl and how it handles enterprise uses... not very well not recommended... sucks its the only option 
            // oh btw the api on pritunl is nonexistant. they tell you to reverse their source to understand what api endpoints you should call.
            // FUCK PRITUNL ESPECIALLY ZACHARY UNDERSTAND OUR PAIN, WE HAD TO DO THIS IN FUCKING PYTHON

            if (!Directory.Exists(Globals.ConfigPath) || force)
            {
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
                var resp = await _apiclient.GetAsync($"https://lightvpn.org/api/configs");
                await CheckResponse(resp);
                var content = await resp.Content.ReadAsStringAsync();

                var configjson = JsonConvert.DeserializeObject<ConfigResponse>(content);

                await File.WriteAllBytesAsync(vpnzip, Convert.FromBase64String(configjson.ConfigArchiveBase64));
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
        public Task<bool> CachedConfigs()
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
            var model = new { username, password };
            var response = await Post<AuthResponse>("https://lightvpn.org/api/auth", model);
            _apiclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{username} {response.Session}");
            return response;
        }

        public async Task<T> Post<T>(string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var resp = await _apiclient.PostAsync(url, content);
            await CheckResponse(resp);
            return JsonConvert.DeserializeObject<T>(await resp.Content.ReadAsStringAsync());
        }
        private static async Task CheckResponse(HttpResponseMessage resp)
        {
            if (resp.StatusCode == HttpStatusCode.Forbidden || resp.StatusCode == HttpStatusCode.TooManyRequests || resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.BadRequest )
            {
                try
                {
                    var content = await resp.Content.ReadAsStringAsync();

                    var errorresp = JsonConvert.DeserializeObject<GenericResponse>(content);

                    throw new InvalidResponseException(errorresp.Message);
                }
                catch (JsonException)
                {
                    throw new InvalidResponseException("You have been ratelimited by Flux");
                }
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
