using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Models;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Services.Interfaces;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class MainViewModel : WindowViewModel
    {
        private ConnectionState _connectionState;

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;
                OnPropertyChanged(nameof(ConnectionState));
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

        public ICommand GridConnectCommand
        {
            get
            {
                return new UiCommand()
                {
                    CommandAction = async (args) =>
                    {
                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                        if (args is not VpnServer server) return;

                        await vpnService.ConnectAsync(server.PritunlName);

                        ConnectionState = ConnectionState.Connecting;
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

                            VpnServers?.Clear();
                            SelectedVpnServer = null;

                            var apiClient = Globals.Container.GetInstance<IApiClient>();
                            var servers = await apiClient.GetAsync<BindingList<VpnServer>>("servers");
                            foreach (var item in servers.OrderByDescending(x => x.CountryName).ThenBy(x => x.ServerName)
                                .ThenBy(x => x.Type))
                                VpnServers.Add(new DisplayVpnServer
                                {
                                    ServerName = item.ServerName,
                                    Id = item.PritunlName,
                                    Type = item.Type,
                                    Status = item.Status ? "Check" : "Close",
                                    Flag =
                                        $"pack://application:,,,/LightVPN.Client.Windows;Component/Resources/Flags/{item.CountryName.Replace(' ', '-')}.png"
                                });
                        }
                        catch (InvalidResponseException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };
            }
        }
    }
}