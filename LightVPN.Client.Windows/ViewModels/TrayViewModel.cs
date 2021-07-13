namespace LightVPN.Client.Windows.ViewModels
{
    using System.Windows;
    using System.Windows.Input;
    using Common;
    using Configuration.Interfaces;
    using Configuration.Models;
    using Discord.Interfaces;
    using Models;
    using OpenVPN.Interfaces;
    using Services.Interfaces;
    using Utils;

    internal sealed class TrayViewModel : BaseViewModel
    {
        public TrayViewModel()
        {
            Globals.TrayViewModel = this;
        }

        public ICommand HideCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        Globals.IsInTray = true;
                        Application.Current.MainWindow?.Hide();
                    },
                    CanExecuteFunc = () => !Globals.IsInTray,
                };
            }
        }

        public ICommand ShowCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        Globals.IsInTray = false;
                        Application.Current.MainWindow?.Show();
                    },
                    CanExecuteFunc = () => Globals.IsInTray,
                };
            }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return new UICommand
                {
                    CanExecuteFunc = () => this.ConnectionState == ConnectionState.Disconnected,
                    CommandAction = async _ =>
                    {
                        if (this.ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting or
                            ConnectionState.Connected) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                        var lastServer = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .LastServer;

                        if (lastServer?.Location == null || lastServer?.PritunlName == null) return;

                        this.ConnectionState = ConnectionState.Connecting;

                        if (Globals.MainViewModel is MainViewModel mainViewModel)
                            mainViewModel.ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {lastServer.Location}...");

                        await vpnService.ConnectAsync(lastServer.PritunlName, lastServer.Location);
                    },
                };
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                return new UICommand
                {
                    CanExecuteFunc = () => this.ConnectionState == ConnectionState.Connected,
                    CommandAction = async _ =>
                    {
                        if (this.ConnectionState != ConnectionState.Connected) return;

                        var vpnManagerService = Globals.Container.GetInstance<IVpnManager>();

                        this.ConnectionState = ConnectionState.Disconnecting;

                        await vpnManagerService.DisconnectAsync();

                        if (Globals.MainViewModel is MainViewModel mainViewModel)
                            mainViewModel.ConnectionState = ConnectionState.Disconnected;

                        this.ConnectionState = ConnectionState.Disconnected;
                    },
                };
            }
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
                this.OnPropertyChanged(nameof(TrayViewModel.ConnectionState));
                this.OnPropertyChanged(nameof(TrayViewModel.TrayIconSource));
            }
        }

        private string _trayIconToolTip = "LightVPN - Disconnected";

        public string TrayIconToolTip
        {
            get => this._trayIconToolTip;
            set
            {
                this._trayIconToolTip = value;
                this.OnPropertyChanged(nameof(TrayViewModel.TrayIconToolTip));
            }
        }

        public string TrayIconSource =>
            this.ConnectionState switch
            {
                ConnectionState.Connected => "Resources/Images/lightvpn-success.ico",
                _ => "Resources/Images/lightvpn-danger.ico",
            };
    }
}
