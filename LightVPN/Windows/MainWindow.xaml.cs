/* --------------------------------------------
 * 
 * Main window - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: MainWindow.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Common.v2.Models;
using LightVPN.Discord.Interfaces;
using LightVPN.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.OpenVPN;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Views;
using LightVPN.Windows;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LightVPN.Auth.Interfaces;
using LightVPN.Settings.Interfaces;
using System.Windows.Threading;
using System.Diagnostics;
using LightVPN.OpenVPN.Models;
using Hardcodet.Wpf.TaskbarNotification;

namespace LightVPN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly IManager _manager;

        public static readonly DependencyProperty IsProcessingProperty =
            DependencyProperty.Register("IsProcessing", typeof(bool),
            typeof(Window), new(false));

        private readonly BeginStoryboard viewLoaded = null;

        private readonly BeginStoryboard viewUnloaded = null;

        private readonly TaskbarIcon notifyIcon;

        public DateTime connectedAt = new();

        public bool IsProcessing
        {
            get { return (bool)GetValue(IsProcessingProperty); }
            set { SetValue(IsProcessingProperty, value); }
        }

        public ConnectionState _connectionState = ConnectionState.Disconnected;

        public string CurrentServer { get; private set; }

        public string CurrentServerId { get; private set; }

        private Page CurrentView;

        private int retryCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            UpdateFooter();
            _manager = Globals.container.GetInstance<IManager>();
            _manager.Connected += Manager_Connected;
            _manager.LoginFailed += LoginFailed;
            _manager.Error += ConnectionError;
            viewLoaded = this.FindResource("ShowFrame") as BeginStoryboard;
            viewUnloaded = this.FindResource("HideFrame") as BeginStoryboard;
            NavigatePage(new Home(this, true));
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        private async void ConnectionError(object sender, string message)
        {
            connectedAt = DateTime.MinValue;
            Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
            _connectionState = ConnectionState.Disconnected;
            await Dispatcher.InvokeAsync(async () =>
            {
                _connectionState = ConnectionState.Disconnected;
                IsProcessing = false;
                UpdateFooter();
                StatusFooter.Content = "Status: Disconnected";
                if (retryCount != 3 && message == "Unknown error connecting to server, reinstall your TAP adapter and try again" || message == "Couldn't find adapter")
                {
                    retryCount++;
                    ShowSnackbar($"Reinstalled TAP Adapter, reconnecting to server now... ({retryCount}/3 attempts)");
                    await ConnectToServerAsync(CurrentServerId, CurrentServer);
                }
                else
                {
                    ShowSnackbar(message);
                    retryCount = 0;
                }

            });
            if (CurrentView is Home home)
            {
                await home.UpdateUIAsync();
            }
        }

        private async void Manager_Connected(object sender)
        {
            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRPC)
            {
                Globals.container.GetInstance<IDiscordRpc>().UpdateTimestamps();
                Globals.container.GetInstance<IDiscordRpc>().UpdateState($"Connected to {CurrentServer}");
            }
            _connectionState = ConnectionState.Connected;
            await Dispatcher.InvokeAsync(async () =>
            {
                IsProcessing = false;
                if (CurrentView is Home home)
                {
                    await home.UpdateUIAsync();
                }
                UpdateFooter();
                StatusFooter.Content = "Status: Connected!";

                retryCount = 0;

                ShowSnackbar($"You are now connected to {CurrentServer}");
            });
            connectedAt = DateTime.Now;
        }

        private async void LoginFailed(object sender)
        {
            connectedAt = DateTime.MinValue;
            if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRPC)
            {
                Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
            }
            _connectionState = ConnectionState.Disconnected;
            await Dispatcher.InvokeAsync(async () =>
            {
                IsProcessing = false;
                if (CurrentView is Home home)
                {
                    await home.UpdateUIAsync();
                }
                UpdateFooter();
                StatusFooter.Content = "Status: Disconnected";
                ShowSnackbar($"Authentication to the server failed");
            });
        }

        private async void CaptionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Content is PackIcon pI)
            {
                switch (pI.Kind)
                {
                    case PackIconKind.WindowClose:
                        if (_connectionState == ConnectionState.Connected)
                        {
                            MessageBoxResult rsltMessageBox = MessageBox.Show("Are you sure you want to close LightVPN?\n\nYour active VPN connection will be terminated!", "LightVPN", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (rsltMessageBox == MessageBoxResult.Yes)
                            {
                                Close();
                            }
                            break;
                        }
                        Close();
                        break;

                    case PackIconKind.WindowMaximize:
                        WindowState = WindowState.Maximized;
                        MaxIcon.Kind = PackIconKind.WindowRestore;
                        break;

                    case PackIconKind.WindowRestore:
                        WindowState = WindowState.Normal;
                        MaxIcon.Kind = PackIconKind.WindowMaximize;
                        break;

                    case PackIconKind.WindowMinimize:
                        WindowState = WindowState.Minimized;
                        break;
                    case PackIconKind.PowerPlugOffOutline:
                        await DisconnectAsync();
                        break;
                }
            }
        }

        public async Task DisconnectAsync(bool notUpdateUi = false)
        {
            connectedAt = DateTime.MinValue;
            if (!notUpdateUi)
            {
                ShowSnackbar($"Disconnected!");
                if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRPC)
                {
                    Globals.container.GetInstance<IDiscordRpc>().ResetTimestamps();
                    Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
                }
            }
            _manager.Disconnect();
            _connectionState = ConnectionState.Disconnected;
            if (CurrentView is Home home)
            {
                await home.UpdateUIAsync();
                home.UpdateViaConnectionState();
            }
            UpdateFooter();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //Handles Windows Aero maximize changes
            if (WindowState == WindowState.Maximized)
            {
                MaxIcon.Kind = PackIconKind.WindowRestore;
            }
            else
            {
                MaxIcon.Kind = PackIconKind.WindowMaximize;
            }
        }

        private async void NavigatePage(Page page)
        {
            if (CurrentView?.Name != page.Name)
            {
                CurrentView = page;
                UpdateFooter();
                viewUnloaded.Storyboard.Begin();
                await Task.Delay(400);
                MainFrame.Navigate(page);
            }
            EnableSideButtons();
        }

        private void UnloadComplete(object sender, EventArgs e)
        {
            viewLoaded.Storyboard.Begin();
        }

        private void PackIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;

            //Get the child objects
            foreach (object child in stackPanel.Children)
            {
                if (child is PackIcon pI)
                {
                    pI.Foreground = new SolidColorBrush(Colors.White);
                }
                break;
            }
        }

        public async Task ConnectToServerAsync(string serverId, string serverName)
        {
            if (_manager.IsConnected) return;
            _connectionState = ConnectionState.Connecting;
            IsProcessing = true;
            CurrentServer = serverName;
            CurrentServerId = serverId;
            string ovpnFn = string.Empty;
            var files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
            if(!files.Any(x => x.Contains(serverId)))
            {
                await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
                files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
            }
            ovpnFn = files.First(x => x.Contains(serverId));

            var existingSettings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
            existingSettings.PreviousServer = new PreviousServer { Id = CurrentServerId, ServerName = CurrentServer };
            
            await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().SaveAsync(existingSettings);
     
            if (CurrentView is Home home)
            {
                home.UpdateViaConnectionState();
                await home.UpdateUIAsync();
            }
            var settings = await Globals.container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();
            if (settings.DiscordRPC)
            {
                Globals.container.GetInstance<IDiscordRpc>().UpdateState("Connecting...");
            }
            ShowSnackbar($"Connecting...");
            UpdateFooter();
            _manager.Connect(ovpnFn);
        }

        private void PackIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;

            //Get the child objects
            foreach (object child in stackPanel.Children)
            {
                if (child is PackIcon pI)
                {
                    pI.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#A5FFFFFF"));
                }
                break;
            }
        }

        public void ShowSnackbar(string message)
        {
            Dispatcher.Invoke(() =>
           {
               var duration = 3;
               MainSnackbar.MessageQueue.Enqueue(
                   $"{message}",
                   null,
                   null,
                   null,
                   false,
                   true,
                   TimeSpan.FromSeconds(duration));
           });
        }

        private void DisableSideButtons()
        {
            // Getting the child objects in the sidebar
            foreach (object child in MainStackpanel.Children)
            {
                if (child is StackPanel panel)
                {
                    panel.MouseDown -= SwitchPage;
                }
            }
        }
        private void EnableSideButtons()
        {
            // Getting the child objects in the sidebar
            foreach (object child in MainStackpanel.Children)
            {
                if (child is StackPanel panel)
                {
                    panel.MouseDown += SwitchPage;
                }
            }
        }


        private void SwitchPage(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;

            DisableSideButtons();

            //Get the child objects
            foreach (object child in stackPanel.Children)
            {
                if (child is PackIcon pI)
                {
                    switch (pI.Kind)
                    {
                        case PackIconKind.HomeOutline:
                            NavigatePage(new Home(this));
                            break;

                        case PackIconKind.ToolboxOutline:
                            NavigatePage(new Tools(this));
                            break;

                        case PackIconKind.SettingsOutline:
                            NavigatePage(new Views.Settings(this));
                            break;

                        case PackIconKind.Logout:
                            MessageBoxResult rsltMessageBox = MessageBox.Show("Are you sure you want to logout?" , "LightVPN", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (rsltMessageBox == MessageBoxResult.Yes)
                            {
                                LoginWindow login = new();
                                login.Show();
                                this.Close();
                            }
                            EnableSideButtons();
                            break;
                    }
                }
                break;
            }
        }

        public void UpdateFooter()
        {
            //ExternalAddress = await Globals.container.GetInstance<IHttp>().GetExternalAddressAsync();
            //IPFooter.Content = $"Click to show IP";
            StatusFooter.Content = $"Status: {_connectionState}";
        }

        private bool ClosingAnimationFinished;

        private void FinishedClosingAnimation(object sender, EventArgs e)
        {
            try
            {
                if (Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load().DiscordRPC)
                {
                    Globals.container.GetInstance<IDiscordRpc>().Dispose();
                }
                _manager.Disconnect();
                _manager.Dispose();
                notifyIcon.Dispose();
            }
            catch (Exception)
            {

            }
            ClosingAnimationFinished = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ClosingAnimationFinished)
            {
                BeginStoryboard sb = this.FindResource("CloseAnim") as BeginStoryboard;
                sb.Storyboard.Begin();
                e.Cancel = true;
            }
        }
    }
}
