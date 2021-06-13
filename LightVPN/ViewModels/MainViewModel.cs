using LightVPN.Auth.Exceptions;
using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.Delegates;
using LightVPN.Discord.Interfaces;
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using LightVPN.Models;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.ViewModels.Base;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    /// <summary>
    /// The view model for the main view, this is a really large view model
    /// </summary>
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken = new();

        private readonly FileLoggerBase _logger = new ErrorLogger();

        private readonly IManager _manager;

        private ConnectionState connectionState = ConnectionState.Disconnected;

        private bool isConnecting;

        private string lastServer;

        private ServersModel selectedServer;

        private BindingList<ServersModel> servers = new();

        public MainViewModel()
        {
            _manager = Globals.Container.GetInstance<IManager>();
            _manager.Connected += Connected;
            _manager.Error += Error;
            var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            LastServer = settings.PreviousServer is null ? "N/A" : $"{settings.PreviousServer?.ServerName} ({settings.PreviousServer?.Type})";
        }

        public ICommand ConnectCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async (args) =>
                    {
                       
                        if (ConnectionState == ConnectionState.Connected)
                        {
                            await DisconnectAsync();
                            return;
                        }

                        var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();
                        var selectedServerId = SelectedServer?.Id ?? settings.PreviousServer?.Id;
                        await ConnectAsync(selectedServerId);
                    },
                    CanExecuteFunc = () => !IsConnecting && LastServer != "N/A"
                };
            }
        }

        public ICommand ConnectCommandArgs
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = async (args) =>
                    {
                        if (args is not ServersModel serversModel) return;

                        if (ConnectionState == ConnectionState.Connecting) return;
                        if (ConnectionState == ConnectionState.Connected) await DisconnectAsync();

                        SaveServer(serversModel.Id, serversModel.ServerName, serversModel.Type);

                        await ConnectAsync(serversModel.Id);
                    },
                    CanExecuteFunc = () => ConnectionState != ConnectionState.Connecting
                };
            }
        }

        public ConnectionState ConnectionState
        {
            get { return connectionState; }

            set
            {
                connectionState = value;
                OnPropertyChanged(nameof(ConnectionState));
            }
        }

        public bool IsConnecting
        {
            get { return isConnecting; }

            set
            {
                isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
            }
        }

        public string LastServer
        {
            get { return lastServer; }

            set
            {
                lastServer = value;
                OnPropertyChanged(nameof(LastServer));
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async (args) =>
                    {
                        try
                        {
                            Servers.Clear();

                            var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                            var servers = await Globals.Container.GetInstance<IHttp>().GetServersAsync(_cancellationToken.Token);

                            foreach (var server in servers.OrderByDescending(x => x.CountryName).ThenBy(x => x.ServerName).ThenBy(x => x.Type))
                            {
                                Servers.Add(new ServersModel
                                {
                                    ServerName = server.ServerName,
                                    Country = server.Location,
                                    Id = server.FileName,
                                    Type = server.Type,
                                    Status = server.Status ? "Check" : "Close",
                                    Flag = $"pack://application:,,,/LightVPN;Component/Resources/Flags/{server.CountryName.Replace(' ', '-')}.png"
                                });
                            }
                            if (settings.AutoConnect && settings.PreviousServer?.Id is not null)
                            {
                                await ConnectAsync(settings.PreviousServer?.Id);
                            }
                        }
                        catch (ClientUpdateRequired)
                        {
                            MessageBox.Show("There's a new version of LightVPN available, please close LightVPN and re-open it to automatically download and install the update", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        catch (RatelimitedException e)
                        {
                            MessageBox.Show($"Couldn't get servers.\n\n{e}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        catch (ApiOfflineException e)
                        {
                            MessageBox.Show($"Couldn't get servers.\n\n{e}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        catch (SubscriptionExpiredException)
                        {
                            MessageBox.Show($"Looks like your subscription to LightVPN has expired, please head over to the dashboard to renew it.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        catch (InvalidResponseException e)
                        {
                            MessageBox.Show($"Couldn't get servers.\n\n{e}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                };
            }
        }

        public ServersModel SelectedServer
        {
            get { return selectedServer; }

            set
            {
                selectedServer = value;
                OnPropertyChanged(nameof(SelectedServer));
            }
        }

        public BindingList<ServersModel> Servers
        {
            get { return servers; }

            set
            {
                servers = value;
                OnPropertyChanged(nameof(Servers));
            }
        }

        public void Dispose()
        {
            LastServer = null;
            IsConnecting = default;
            Servers = null;
            _manager.Dispose();
            _cancellationToken.Cancel();
            GC.SuppressFinalize(this);
        }

        internal async Task ConnectAsync(string serverName)
        {
            try
            {
                if (servers != null && Servers.First(x => x.Id == serverName).Status == "Close")
                {
                    MessageBox.Show("This server is explicitly offline, please try again later.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception e)
            {
                await _logger.WriteAsync($"Bandaid Solution was triggered: {serverName}");
            }
            if (_manager.IsConnected || ConnectionState == ConnectionState.Connecting) return;

            ConnectionState = ConnectionState.Connecting;
            IsConnecting = true;

            try
            {
                // This processes and finds the config on the filesystem
                string ovpnFn = string.Empty;
                var files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));

                if (!files.Any(x => x.Contains(serverName)))
                {
                    // Config wasn't found, so instead we re-cache the configs to hope that the
                    // server has the new config
                    await Globals.Container.GetInstance<IHttp>().CacheConfigsAsync(true, _cancellationToken.Token);
                    files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
                }

                ovpnFn = files.First(x => x.Contains(serverName));

                if (string.IsNullOrWhiteSpace(ovpnFn))
                {
                    MessageBox.Show("Failed to locate the server configuration file, therefore connection to the server was aborted.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Globals.Container.GetInstance<IDiscordRpc>().UpdateState("Connecting...");
                if (Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
                {
                    Globals.Container.GetInstance<IDiscordRpc>().SetPresence();
                }

                await _manager.ConnectAsync(ovpnFn);
            }
            catch (InvalidOperationException e)
            {
                // This for if the config wasn't found, we recache and reinitiate the connection.
                await _logger.WriteAsync($"Couldn't find config, ignored exception and auto-troubleshooting...\n\n{e}");

                await _manager.PerformAutoTroubleshootAsync(true, "Failed to find configuration file for that server. The server could no longer be active on LightVPN", _cancellationToken.Token);
            }
        }

        internal async Task DisconnectAsync()
        {
            IsConnecting = true;
            ConnectionState = ConnectionState.Disconnecting;
            Globals.Container.GetInstance<IDiscordRpc>().ResetTimestamps();
            Globals.Container.GetInstance<IDiscordRpc>().ResetPresence();

            if (Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.Container.GetInstance<IDiscordRpc>().SetPresence();
            }
                await _manager.DisconnectAsync();
            IsConnecting = false;
            ConnectionState = ConnectionState.Disconnected;
        }

        internal void SaveServer(string pritunlName, string friendlyName, ServerType serverType)
        {
            var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();

            settings.PreviousServer = new()
            {
                ServerName = friendlyName,
                Id = pritunlName,
                Type = serverType
            };

            Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Save(settings);
            Globals.Container.GetInstance<IDiscordRpc>().UpdateTimestamps();
            Globals.Container.GetInstance<IDiscordRpc>().UpdateState($"Connected to {friendlyName} ({serverType})");
            if (Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.Container.GetInstance<IDiscordRpc>().SetPresence();
            }

            LastServer = $"{friendlyName} ({serverType})";
        }

        private void Connected(object sender)
        {
            IsConnecting = false;
            ConnectionState = ConnectionState.Connected;
            Globals.Container.GetInstance<IDiscordRpc>().UpdateState($"Connected!");
            Globals.Container.GetInstance<IDiscordRpc>().UpdateTimestamps();
            if (Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.Container.GetInstance<IDiscordRpc>().SetPresence();
            }
        }

        private void Error(object sender, string message)
        {
            MessageBox.Show($"A problem has occurred whilst connecting to the server;\n\n{message}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
            IsConnecting = false;
            ConnectionState = ConnectionState.Disconnected;
        }

        public class ServersModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Country
            {
                get
                {
                    return _country;
                }

                set
                {
                    _country = value;
                    OnPropertyChanged(nameof(Country));
                }
            }

            public string Flag
            {
                get
                {
                    return _flag;
                }

                set
                {
                    _flag = value;
                    OnPropertyChanged(nameof(Flag));
                }
            }

            public string Id
            {
                get
                {
                    return _id;
                }

                set
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string ServerName
            {
                get
                {
                    return _displayName;
                }

                set
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(ServerName));
                }
            }

            public string Status
            {
                get
                {
                    return _status;
                }

                set
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }

            public ServerType Type
            {
                get
                {
                    return _type;
                }

                set
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }

            private string _country { set; get; }

            private string _displayName { set; get; }

            private string _flag { set; get; }

            private string _id { set; get; }

            private string _status { set; get; }

            private ServerType _type { set; get; }

            private void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged is null) return;

                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}