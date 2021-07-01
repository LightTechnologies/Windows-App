using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Discord.Interfaces;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Services.Interfaces;
using LightVPN.Client.Windows.Utils;

namespace LightVPN.Client.Windows.ViewModels
{
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
                    CanExecuteFunc = () => !Globals.IsInTray
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
                    CanExecuteFunc = () => Globals.IsInTray
                };
            }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return new UICommand
                {
                    CanExecuteFunc = () => ConnectionState == ConnectionState.Disconnected,
                    CommandAction = async _ =>
                    {
                        if (ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting or
                            ConnectionState.Connected) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                        var lastServer = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .LastServer;

                        if (lastServer?.Location == null || lastServer?.PritunlName == null) return;

                        ConnectionState = ConnectionState.Connecting;

                        if (Globals.MainViewModel is MainViewModel mainViewModel)
                            mainViewModel.ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {lastServer.Location}...");

                        await vpnService.ConnectAsync(lastServer.PritunlName, lastServer.Location);
                    }
                };
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                return new UICommand
                {
                    CanExecuteFunc = () => ConnectionState == ConnectionState.Connected,
                    CommandAction = async _ =>
                    {
                        if (ConnectionState != ConnectionState.Connected) return;

                        var vpnManagerService = Globals.Container.GetInstance<IVpnManager>();

                        ConnectionState = ConnectionState.Disconnecting;

                        await vpnManagerService.DisconnectAsync();

                        if (Globals.MainViewModel is MainViewModel mainViewModel)
                            mainViewModel.ConnectionState = ConnectionState.Disconnected;

                        ConnectionState = ConnectionState.Disconnected;
                    }
                };
            }
        }

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
                OnPropertyChanged(nameof(TrayIconSource));
            }
        }

        private string _trayIconToolTip = "LightVPN - Disconnected";

        public string TrayIconToolTip
        {
            get => _trayIconToolTip;
            set
            {
                _trayIconToolTip = value;
                OnPropertyChanged(nameof(TrayIconToolTip));
            }
        }

        public string TrayIconSource =>
            ConnectionState switch
            {
                ConnectionState.Connected => "Resources/Images/lightvpn-success.ico",
                _ => "Resources/Images/lightvpn-danger.ico"
            };
    }
}
