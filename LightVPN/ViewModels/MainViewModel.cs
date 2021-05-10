using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.Delegates;
using LightVPN.Discord.Interfaces;
using LightVPN.Models;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.ViewModels.Base;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly IManager _manager;

        private ConnectionState connectionState = ConnectionState.Disconnected;

        private bool isConnecting;

        private string lastServer;

        private ServersModel selectedServer;

        private BindingList<ServersModel> servers = new();

        public MainViewModel()
        {
            _manager = Globals.container.GetInstance<IManager>();
            _manager.Connected += Connected;
            _manager.Error += Error;
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
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

                        var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        await ConnectAsync(settings.PreviousServer?.Id);
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
                        if (ConnectionState == ConnectionState.Connecting) return;
                        if (ConnectionState == ConnectionState.Connected)
                        {
                            await DisconnectAsync();
                        }

                        if (args is not ServersModel serversModel)
                        {
                            return;
                        }

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
                        Servers.Clear();

                        var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        var servers = await Globals.container.GetInstance<IHttp>().GetServersAsync();

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

                            if (settings.AutoConnect && settings.PreviousServer?.Id is not null)
                            {
                                await ConnectAsync(settings.PreviousServer?.Id);
                            }
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
            GC.SuppressFinalize(this);
        }

        internal async Task ConnectAsync(string serverName)
        {
            if (_manager.IsConnected || ConnectionState == ConnectionState.Connecting) return;

            ConnectionState = ConnectionState.Connecting;
            IsConnecting = true;

            // This processes and finds the config on the filesystem
            string ovpnFn = string.Empty;
            var files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));

            if (!files.Any(x => x.Contains(serverName)))
            {
                // Config wasn't found, so instead we re-cache the configs to hope that the server
                // has the new config
                await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
                files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
            }

            ovpnFn = files.First(x => x.Contains(serverName));

            if (string.IsNullOrWhiteSpace(ovpnFn))
            {
                MessageBox.Show("Failed to locate the server configuration file, therefore connection to the server was aborted.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.container.GetInstance<IDiscordRpc>().UpdateState("Connecting...");
            }

            _manager.Connect(ovpnFn);
        }

        internal async Task DisconnectAsync()
        {
            IsConnecting = true;
            ConnectionState = ConnectionState.Disconnecting;
            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.container.GetInstance<IDiscordRpc>().ResetTimestamps();
                Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
            }
            await Task.Run(() =>
            {
                _manager.Disconnect();
            });
            IsConnecting = false;
            ConnectionState = ConnectionState.Disconnected;
        }

        internal void SaveServer(string pritunlName, string friendlyName, ServerType serverType)
        {
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

            settings.PreviousServer = new()
            {
                ServerName = friendlyName,
                Id = pritunlName,
                Type = serverType
            };

            Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Save(settings);

            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.container.GetInstance<IDiscordRpc>().UpdateTimestamps();
                Globals.container.GetInstance<IDiscordRpc>().UpdateState($"Connected to {friendlyName} ({serverType})");
            }

            LastServer = $"{friendlyName} ({serverType})";
        }

        private void Connected(object sender)
        {
            IsConnecting = false;
            ConnectionState = ConnectionState.Connected;
            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRpc)
            {
                Globals.container.GetInstance<IDiscordRpc>().UpdateState($"Connected!");
                Globals.container.GetInstance<IDiscordRpc>().ResetTimestamps();
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
                var saved = PropertyChanged;
                if (saved != null)
                {
                    var e = new PropertyChangedEventArgs(propertyName);
                    saved(this, e);
                }
            }
        }
    }
}