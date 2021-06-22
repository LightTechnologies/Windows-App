 using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Discord.Interfaces;
using LightVPN.Client.OpenVPN.EventArgs;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Models;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Services.Interfaces;
using LightVPN.Client.Windows.Views;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class MainViewModel : WindowViewModel
    {
        public MainViewModel() : base(true)
        {
        }
        
        // Keep a time since the servers were last cached
        private DateTime LastCache { get; set; }


        private ConnectionState _connectionState;

        public ConnectionState ConnectionState
        {
            get
            {
                if (Globals.Container.GetInstance<IVpnManager>().IsConnected)
                    _connectionState = ConnectionState.Connected;
                else if (_connectionState != ConnectionState.Connecting)
                    _connectionState = ConnectionState.Disconnected;

                return _connectionState;
            }
            set
            {
                _connectionState = value;
                OnPropertyChanged(nameof(ConnectionState));
            }
        }

        private string _lastServer;

        public string LastServer
        {
            get => _lastServer;
            set
            {
                _lastServer = value;
                OnPropertyChanged(nameof(LastServer));
            }
        }

        private DisplayVpnServer _selectedVpnServer;

        public DisplayVpnServer SelectedVpnServer
        {
            get => _selectedVpnServer;
            set
            {
                _selectedVpnServer = value;
                OnPropertyChanged(nameof(SelectedVpnServer));
            }
        }

        private BindingList<DisplayVpnServer> _vpnServers;

        public BindingList<DisplayVpnServer> VpnServers
        {
            get => _vpnServers;
            set
            {
                _vpnServers = value;
                OnPropertyChanged(nameof(VpnServers));
            }
        }

        public ICommand ToggleConnectCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async _ =>
                    {
                        if (ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();
                        var vpnManagerService = Globals.Container.GetInstance<IVpnManager>();

                        if (ConnectionState == ConnectionState.Connected)
                        {
                            ConnectionState = ConnectionState.Disconnecting;
                            await vpnManagerService.DisconnectAsync();
                            ConnectionState = ConnectionState.Disconnected;
                            return;
                        }

                        var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                            .Read();

                        if (string.IsNullOrWhiteSpace(config.LastServer?.Location) ||
                            string.IsNullOrWhiteSpace(config.LastServer.PritunlName)) return;

                        ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {config.LastServer?.Location}...");

                        await vpnService.ConnectAsync(config.LastServer.PritunlName, config.LastServer.Location);
                    }
                };
            }
        }

        public ICommand GridConnectCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async args =>
                    {
                        if (ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting or
                            ConnectionState.Connected) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                        if (args is not DisplayVpnServer server) return;

                        LastServer = server.ServerName;

                        ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {server.ServerName}...");

                        await vpnService.ConnectAsync(server.Id, server.ServerName);
                    }
                };
            }
        }

        public ICommand LoadSettingsCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = _ =>
                    {
                        var mainWindow = (MainWindow)Application.Current.MainWindow;
                        mainWindow?.LoadView(new SettingsView());
                    }
                };
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            VpnServers ??= new BindingList<DisplayVpnServer>();

                            SelectedVpnServer = null;

                            if (DateTime.Now < LastCache.AddHours(1)) return;

                            var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                                .Read();
                            LastServer = config.LastServer is null ? "N/A" : config.LastServer.Location;

                            var apiClient = Globals.Container.GetInstance<IApiClient>();
                            var servers = await apiClient.GetAsync<BindingList<VpnServer>>("servers");

                            var vpnService = Globals.Container.GetInstance<IVpnManager>();
                            vpnService.OnConnected += OnConnected;
                            vpnService.OnErrorReceived += OnErrorReceived;

                            VpnServers.Clear();

                            foreach (var item in servers.OrderByDescending(x => x.CountryName).ThenBy(x => x.ServerName)
                                .ThenBy(x => x.Type))
                                VpnServers.Add(new DisplayVpnServer
                                {
                                    ServerName = item.ServerName,
                                    Id = item.PritunlName,
                                    Type = item.Type,
                                    Country = item.CountryName,
                                    Status = item.Status ? "Check" : "Close",
                                    Flag =
                                        $"pack://application:,,,/LightVPN.Client.Windows;Component/Resources/Flags/{item.CountryName.Replace(' ', '-')}.png"
                                });

                            LastCache = DateTime.Now;
                        }
                        catch (InvalidResponseException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };
            }
        }

        private void OnErrorReceived(object sender, ErrorEventArgs e)
        {
            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                .IsDiscordRpcEnabled)
                Globals.Container.GetInstance<IDiscordRp>()
                    .UpdateState("Disconnected");

            ConnectionState = ConnectionState.Disconnected;

            MessageBox.Show(e.Exception.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnConnected(object sender, ConnectedEventArgs e)
        {
            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                .IsDiscordRpcEnabled)
                Globals.Container.GetInstance<IDiscordRp>()
                    .UpdateState("Connected!");
            ConnectionState = ConnectionState.Connected;
        }
    }
}