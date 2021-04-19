/* --------------------------------------------
 * 
 * Home view - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: Home.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LightVPN.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.v2.Models;
using LightVPN.Auth.Interfaces;
using LightVPN.Settings.Interfaces;
using static LightVPN.Auth.ApiException;
using System;
using LightVPN.Logger.Base;
using LightVPN.Logger;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Data;
using LightVPN.Discord.Interfaces;
using System.Threading;
using LightVPN.Common.v2;
using System.Windows.Threading;
using LightVPN.OpenVPN.Models;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private readonly BindingList<ServersGrid> gridObjects = new();

        private readonly MainWindow _host;

        private readonly bool _wasFirstLoad;

        private readonly FileLogger _logger = new ErrorLogger(Globals.ErrorLogPath);

        public static readonly RoutedUICommand ToggleConnectCommand =
        new("Connect",
                            "ToggleConnectCommand",
                            typeof(Home));

        public static readonly DependencyProperty IsProcessingTwoProperty =
            DependencyProperty.Register("IsProcessingTwo", typeof(bool),
            typeof(Home), new(false));

        public bool IsProcessingTwo
        {
            get { return (bool)GetValue(IsProcessingTwoProperty); }
            set { SetValue(IsProcessingTwoProperty, value); }
        }

        public Home(MainWindow host, bool wasFirstLoad = false)
        {
            InitializeComponent();
            _host = host;
            _wasFirstLoad = wasFirstLoad;
            if (_host._connectionState == ConnectionState.Connected)
            {
                var connectionElapsed = (DateTime.Now - _host.connectedAt).ToHMS();
                connectionElapsed = string.IsNullOrWhiteSpace(connectionElapsed) ? "0 seconds" : connectionElapsed;
                LastServerTitle.Text = $"Connected for {connectionElapsed}";
            }
            else if (_host._connectionState == ConnectionState.Disconnected)
            {
                LastServerTitle.Text = "Your last server";
            }

            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (object sender, EventArgs e) =>
            {
                if (_host._connectionState == ConnectionState.Connected)
                {
                    var connectionElapsed = (DateTime.Now - _host.connectedAt).ToHMS();
                    connectionElapsed = string.IsNullOrWhiteSpace(connectionElapsed) ? "0 seconds" : connectionElapsed;
                    LastServerTitle.Text = $"Connected for {connectionElapsed}";
                }
                else
                {
                    LastServerTitle.Text = "Your last server";
                }
            }, Dispatcher);
            timer.Start();
            this.CommandBindings.Add(new CommandBinding(ToggleConnectCommand, ToggleConnectCommand_Event));
            
        }

        private async void ToggleConnectCommand_Event(object sender, ExecutedRoutedEventArgs e)
        {
            if (_host._connectionState == ConnectionState.Connected)
            {
                _host.connectedAt = DateTime.MinValue;
                _host.ShowSnackbar($"Disconnected!");
                Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
                _host._connectionState = ConnectionState.Disconnected;
                UpdateViaConnectionState();
                _host._manager.Disconnect();
                await UpdateUIAsync();
                _host.UpdateFooter();
            }
            else
            {
                var existingSettings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
                await UpdateUIAsync();
                await _host.ConnectToServerAsync(existingSettings.PreviousServer.Id, existingSettings.PreviousServer.ServerName);
            }
        }

        public async Task UpdateUIAsync()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var settings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
                RecentServer.Text = settings.PreviousServer.ServerName ?? "N/A";
                UpdateViaConnectionState();
            });
        }

        internal void UpdateViaConnectionState()
        {
            switch (_host._connectionState)
            {
                case ConnectionState.Connected:
                    RecentConnectButton.IsEnabled = true;
                    ConnectButtonIcon.Kind = PackIconKind.PowerPlugOffOutline;
                    ConnectButtonText.Text = " DISCONNECT";
                    _host.IsProcessing = false;
                    IsProcessingTwo = false;
                    break;
                case ConnectionState.Connecting:
                    RecentConnectButton.IsEnabled = false;
                    ConnectButtonText.Text = " CONNECTING";
                    IsProcessingTwo = true;
                    _host.IsProcessing = true;
                    break;
                case ConnectionState.Disconnected:
                    RecentConnectButton.IsEnabled = true;
                    ConnectButtonIcon.Kind = PackIconKind.PowerPlugOutline;
                    ConnectButtonText.Text = " CONNECT";
                    IsProcessingTwo = false;
                    _host.IsProcessing = false;
                    break;
            }
            _host.StatusFooter.Content = $"Status: {_host._connectionState}";
        }

        private async Task DownloadServers()
        {
            var settings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
            RecentServer.Text = settings.PreviousServer?.ServerName ?? "N/A";

            UpdateViaConnectionState();

            RecentConnectButton.IsEnabled = settings.PreviousServer is not null;

            var servers = await Globals.container.GetInstance<IHttp>().GetServersAsync();
            foreach (var server in servers.OrderByDescending(x => x.Type).ThenBy(x => x.ServerName))
            {
                gridObjects.Add(new ServersGrid { ServerName = server.ServerName, Country = server.Location, Server = server.FileName, Id = server.Id, Type = server.Type, Flag = $"pack://application:,,,/LightVPN;Component/Resources/Flags/{server.Country.Replace(' ', '-')}.png"
            });
            }
            if (settings.PreviousServer is not null && settings.AutoConnect && gridObjects.Any(x => x.Server == settings.PreviousServer?.Id) && _wasFirstLoad)
            {
                var server = gridObjects.FirstOrDefault(x => x.Server == settings.PreviousServer?.Id);
                await _host.ConnectToServerAsync(server.Server, server.ServerName);
                await UpdateUIAsync();
                return;
            }
            _host.IsProcessing = false;
        }

        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void Grid_MouseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ConnectItem_Click(null, null);
        }

        private async Task InitalizeViewAsync()
        {
            MaterialDesignThemes.Wpf.DataGridTextColumn @column1 = new();
            MaterialDesignThemes.Wpf.DataGridTextColumn @column2 = new();

            ServerList.Columns.Add(@column1);
            ServerList.Columns.Add(@column2);

            @column1.Binding = new Binding("ServerName");
            @column2.Binding = new Binding("Type");

            @column1.Header = "Server";
            @column2.Header = "Type";

            ServerList.ItemsSource = gridObjects;

            try
            {
                await DownloadServers();
            }
            catch (RatelimitedException e)
            {
                _host.ShowSnackbar(e.Message);
            }
            catch (ApiOfflineException e)
            {
                _host.ShowSnackbar(e.Message);
            }
            catch (InvalidResponseException e)
            {
                _host.ShowSnackbar(e.Message);
            }
            catch (Exception e)
            {
                await _logger.WriteAsync(e.ToString());
                _host.ShowSnackbar("Couldn't get servers");
            }
        }

        public async void ConnectItem_Click(object sender, RoutedEventArgs e)
        {
            if (_host._connectionState == ConnectionState.Connected)
            {
                await _host.DisconnectAsync(true);
            }
            if (_host._connectionState == ConnectionState.Disconnected || _host._connectionState == ConnectionState.Connecting)
            {
                if (ServerList.SelectedItem is not null)
                {
                    var item = (ServersGrid)ServerList.SelectedItem;
                    var existingSettings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
                    await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().SaveAsync(new SettingsModel
                    {
                        AutoConnect = existingSettings.AutoConnect,
                        PreviousServer = new PreviousServer
                        {
                            Id = item.Server,
                            ServerName = item.ServerName
                        },
                        KillSwitch = existingSettings.KillSwitch,
                        DarkMode = existingSettings.DarkMode
                    });
                    await UpdateUIAsync();
                    await _host.ConnectToServerAsync(item.Server, item.ServerName);
                }
                else
                {
                    _host.ShowSnackbar("No server was selected, click on a server and try connecting again");
                }
            }
        }

        private async void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            _host.IsProcessing = true;

            await InitalizeViewAsync();
        }
    }
}
