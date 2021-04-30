using LightVPN.Auth.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.Models;
using LightVPN.Discord.Interfaces;
using LightVPN.Models;
using LightVPN.OpenVPN;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IManager _manager;

        public MainViewModel()
        {
            _manager = Globals.container.GetInstance<IManager>();
            _manager.Connected += Connected;
            _manager.Error += Error;
            _manager.LoginFailed += LoginFailed;
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            LastServer = settings.PreviousServer is null ? "N/A" : $"{settings.PreviousServer?.ServerName} ({settings.PreviousServer?.Type})";
        }

        private void LoginFailed(object sender)
        {
            MessageBox.Show($"A problem has occurred whilst authenticating with the server, please try again.\n\nIf the problem persists, please contact LightVPN support.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
            IsConnecting = false;
            ConnectionState = ConnectionState.Disconnected;
        }

        private void Error(object sender, string message)
        {
            MessageBox.Show($"A problem has occurred whilst connecting to the server;\n\n{message}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
            IsConnecting = false;
            ConnectionState = ConnectionState.Disconnected;
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

        private bool isConnecting;
        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
            }
        }

        private BindingList<ServersModel> servers = new();
        public BindingList<ServersModel> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
                OnPropertyChanged(nameof(Servers));
            }
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
                // Config wasn't found, so instead we re-cache the configs to hope that the server has the new config
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

        internal async Task DisconnectAsync()
        {
            IsConnecting = true;
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

        public ICommand ConnectCommandArgs
        {
            get
            {
                return new ConnectDelegate()
                {
                    CommandAction = async (args) =>
                    {
                        if (ConnectionState == ConnectionState.Connected)
                        {
                            await DisconnectAsync();
                        }

                        SaveServer(args.Server, args.ServerName, args.Type);

                        await ConnectAsync(args.Server);
                    }
                };
            }
        }

        private ConnectionState connectionState = ConnectionState.Disconnected;
        public ConnectionState ConnectionState
        {
            get { return connectionState; }
            set
            {
                connectionState = value;
                OnPropertyChanged(nameof(ConnectionState));
            }
        }

        private string lastServer;
        public string LastServer
        {
            get { return lastServer; }
            set
            {
                lastServer = value;
                OnPropertyChanged(nameof(LastServer));
            }
        
        }

        private ServersModel selectedServer;
        public ServersModel SelectedServer
        {
            get { return selectedServer; }
            set
            {
                selectedServer = value;
                OnPropertyChanged(nameof(SelectedServer));
            }
        }

        public struct ServersModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string displayName { set; get; }
            private string country { set; get; }

            private string flag { set; get; }

            private string id { set; get; }

            private IPAddress ipAddress { set; get; }

            private string server { set; get; }

            private ServerType type { set; get; }

            private string status { set; get; }

            private long usersConnected { set; get; }

            public string ServerName
            {
                get
                {
                    return displayName;
                }

                set
                {
                    displayName = value;
                    OnPropertyChanged(nameof(ServerName));
                }
            }

            public string Status
            {
                get
                {
                    return status;
                }

                set
                {
                    status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }

            public string Country
            {
                get
                {
                    return country;
                }

                set
                {
                    country = value;
                    OnPropertyChanged(nameof(Country));
                }
            }

            public string Flag
            {
                get
                {
                    return flag;
                }

                set
                {
                    flag = value;
                    OnPropertyChanged(nameof(Flag));
                }
            }
            public string Id
            {
                get
                {
                    return id;
                }

                set
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
            public IPAddress IpAddress
            {
                get
                {
                    return ipAddress;
                }

                set
                {
                    ipAddress = value;
                    OnPropertyChanged(nameof(IpAddress));
                }
            }

            public long UsersConnected
            {
                get
                {
                    return usersConnected;
                }

                set
                {
                    usersConnected = value;
                    OnPropertyChanged(nameof(UsersConnected));
                }
            }

            public string Server
            {
                get
                {
                    return server;
                }

                set
                {
                    server = value;
                    OnPropertyChanged(nameof(Server));
                }
            }

            public ServerType Type
            {
                get
                {
                    return type;
                }

                set
                {
                    type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }

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

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async () =>
                    {
                        Servers.Clear();

                        var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        var servers = await Globals.container.GetInstance<IHttp>().GetServersAsync();

                        foreach (var server in servers.OrderByDescending(x => x.Country).ThenBy(x => x.ServerName).ThenBy(x => x.Type))
                        {
                            Servers.Add(new ServersModel
                            {
                                ServerName = server.ServerName,
                                Country = server.Location,
                                Server = server.FileName,
                                Id = server.Id,
                                Type = server.Type,
                                UsersConnected = server.DevicesOnline,
                                Status = server.Status ? "Check" : "Close",
                                Flag = $"pack://application:,,,/LightVPN;Component/Resources/Flags/{server.Country.Replace(' ', '-')}.png"
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

        public ICommand ConnectCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async () =>
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}