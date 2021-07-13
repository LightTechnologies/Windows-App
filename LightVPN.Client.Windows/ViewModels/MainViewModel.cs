namespace LightVPN.Client.Windows.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Auth.Exceptions;
    using Auth.Interfaces;
    using Auth.Models;
    using Common;
    using Common.Models;
    using Configuration.Interfaces;
    using Configuration.Models;
    using Discord.Interfaces;
    using MaterialDesignThemes.Wpf;
    using Models;
    using OpenVPN.EventArgs;
    using OpenVPN.Interfaces;
    using Services.Interfaces;
    using Utils;
    using Views;

    internal sealed class MainViewModel : WindowViewModel
    {
        public MainViewModel()
        {
            Globals.MainViewModel = this;
        }

        private ConnectionState _connectionState;

        public ConnectionState ConnectionState
        {
            get
            {
                if (Globals.Container.GetInstance<IVpnManager>().IsConnected)
                    this._connectionState = ConnectionState.Connected;
                else if (this._connectionState != ConnectionState.Connecting)
                    this._connectionState = ConnectionState.Disconnected;

                return this._connectionState;
            }
            set
            {
                this._connectionState = value;
                this.OnPropertyChanged(nameof(MainViewModel.ConnectionState));
            }
        }

        private string _lastServer;

        public string LastServer
        {
            get => this._lastServer;
            set
            {
                this._lastServer = value;
                this.OnPropertyChanged(nameof(MainViewModel.LastServer));
            }
        }

        private DisplayVpnServer _selectedVpnServer;

        public DisplayVpnServer SelectedVpnServer
        {
            get => this._selectedVpnServer;
            set
            {
                this._selectedVpnServer = value;
                this.OnPropertyChanged(nameof(MainViewModel.SelectedVpnServer));
            }
        }

        private BindingList<DisplayVpnServer> _vpnServers;

        public BindingList<DisplayVpnServer> VpnServers
        {
            get => this._vpnServers;
            set
            {
                this._vpnServers = value;
                this.OnPropertyChanged(nameof(MainViewModel.VpnServers));
            }
        }

        public ICommand ToggleConnectCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            if (this.ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting) return;

                            var vpnService = Globals.Container.GetInstance<IOpenVpnService>();
                            var vpnManagerService = Globals.Container.GetInstance<IVpnManager>();

                            if (this.ConnectionState == ConnectionState.Connected)
                            {
                                this.ConnectionState = ConnectionState.Disconnecting;

                                await vpnManagerService.DisconnectAsync();

                                if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                                    trayViewModel.ConnectionState = ConnectionState.Disconnected;

                                this.ConnectionState = ConnectionState.Disconnected;

                                return;
                            }

                            var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                                .Read();

                            if (string.IsNullOrWhiteSpace(config.LastServer?.Location) ||
                                string.IsNullOrWhiteSpace(config.LastServer.PritunlName)) return;

                            this.ConnectionState = ConnectionState.Connecting;

                            if (Globals.TrayViewModel is TrayViewModel trayViewModel1)
                                trayViewModel1.ConnectionState = ConnectionState.Connecting;

                            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                                .IsDiscordRpcEnabled)
                                Globals.Container.GetInstance<IDiscordRp>()
                                    .UpdateState($"Connecting to {config.LastServer?.Location}...");

                            await vpnService.ConnectAsync(config.LastServer.PritunlName, config.LastServer.Location);
                        }
                        catch (InvalidOperationException e)
                        {
                            this.ConnectionState = ConnectionState.Disconnected;

                            await DialogManager.ShowDialogAsync(
                                PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
                        }
                    },
                };
            }
        }

        public ICommand GridConnectCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async args =>
                    {
                        try
                        {
                            if (this.ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting or
                                ConnectionState.Connected) return;

                            var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                            if (args is not DisplayVpnServer server) return;

                            this.LastServer = server.ServerName;

                            this.ConnectionState = ConnectionState.Connecting;

                            if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                                trayViewModel.ConnectionState = ConnectionState.Connecting;

                            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                                .IsDiscordRpcEnabled)
                                Globals.Container.GetInstance<IDiscordRp>()
                                    .UpdateState($"Connecting to {server.ServerName}...");

                            await vpnService.ConnectAsync(server.Id, server.ServerName);
                        }
                        catch (InvalidOperationException e)
                        {
                            this.ConnectionState = ConnectionState.Disconnected;

                            await DialogManager.ShowDialogAsync(
                                PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
                        }
                    },
                };
            }
        }

        public ICommand LoadSettingsCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        this.IsPlayingAnimation = true;
                        var mainWindow = (MainWindow) Application.Current.MainWindow;
                        mainWindow?.LoadView(new SettingsView());
                    },
                    CanExecuteFunc = () => !this.IsPlayingAnimation,
                };
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            this.IsPlayingAnimation = false;

                            this.VpnServers ??= new BindingList<DisplayVpnServer>();
                            this.SelectedVpnServer = null;

                            var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                                .Read();

                            this.LastServer = config.LastServer is null ? "N/A" : config.LastServer.Location;

                            var vpnService = Globals.Container.GetInstance<IVpnManager>();
                            vpnService.OnConnected += this.OnConnected;
                            vpnService.OnErrorReceived += this.OnErrorReceived;

                            var cacheService = Globals.Container.GetInstance<ICacheService>();

                            var cachedVpnServers = await cacheService.GetCachedApiServerResponseAsync();
                            if (cachedVpnServers is not null)
                            {
                                this.VpnServers = cachedVpnServers;
                                return;
                            }

                            var apiClient = Globals.Container.GetInstance<IApiClient>();
                            var servers = await apiClient.GetAsync<BindingList<VpnServer>>("servers");

                            this.VpnServers.Clear();

                            foreach (var item in servers.OrderByDescending(x => x.CountryName).ThenBy(x => x.ServerName)
                                .ThenBy(x => x.Type))
                                this.VpnServers.Add(new DisplayVpnServer
                                {
                                    ServerName = item.ServerName,
                                    Id = item.PritunlName,
                                    Type = item.Type,
                                    Country = item.CountryName,
                                    Status = item.Status ? "Check" : "Close",
                                    Flag =
                                        $"pack://application:,,,/Resources/Flags/{item.CountryName.Replace(' ', '-')}.png",
                                });

                            await cacheService.CacheApiServerResponseAsync(this.VpnServers);
                        }
                        catch (InvalidResponseException e)
                        {
                            await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
                        }
                    },
                };
            }
        }

        private async void OnErrorReceived(object sender, ErrorEventArgs e)
        {
            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                .IsDiscordRpcEnabled)
                Globals.Container.GetInstance<IDiscordRp>()
                    .UpdateState("Disconnected");

            this.ConnectionState = ConnectionState.Disconnected;

            if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                trayViewModel.ConnectionState = ConnectionState.Disconnected;

            // Since the DialogHost requires STAThread, we must invoke this method using the dispatcher.
            await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
                PackIconKind.ErrorOutline, "Something went wrong...",
                e.Exception.Message));
        }

        private void OnConnected(object sender, ConnectedEventArgs e)
        {
            if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                .IsDiscordRpcEnabled)
                Globals.Container.GetInstance<IDiscordRp>()
                    .UpdateState("Connected!");

            if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                trayViewModel.ConnectionState = ConnectionState.Connected;

            this.ConnectionState = ConnectionState.Connected;
        }
    }
}
