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

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool),
            typeof(Window), new(false));

        private readonly BeginStoryboard viewLoaded = null;

        private readonly BeginStoryboard viewUnloaded = null;

        public DateTime connectedAt = new();

        public bool IsProcessing
        {
            get { return (bool)GetValue(IsProcessingProperty); }
            set { SetValue(IsProcessingProperty, value); }
        }

        public bool IsConnecting = false;
        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        private string ExternalAddress = "127.0.0.1";

        public string CurrentServer { get; private set; }

        public string CurrentServerId { get; private set; }

        private Page CurrentView;

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
            viewLoaded = this.FindResource("ShowFrame") as BeginStoryboard; // push this update
            viewUnloaded = this.FindResource("HideFrame") as BeginStoryboard;
            NavigatePage(new Home(this, true));
        }

        private async void ConnectionError(object sender, string message)
        {
            connectedAt = DateTime.MinValue;
            await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
            {
                State = "Disconnected"
            });
            IsConnecting = false;
            await Dispatcher.InvokeAsync(async () => //  make kill switch u wont
            {
                IsConnected = false;
                IsProcessing = false;
                UpdateFooter();
                StatusFooter.Content = "Status: Disconnected";
                if (message == "Unknown error connecting to server, reinstall your TAP adapter and try again" || message == "Couldn't find adapter")
                {
                    Globals.container.GetInstance<ITapManager>().RemoveTapAdapter();
                    Globals.container.GetInstance<ITapManager>().CreateTapAdapter();
                    ShowSnackbar("Reinstalled TAP Adapter, please connect again");
                }
                else
                {
                    ShowSnackbar(message);
                }

            });
            if (CurrentView is Home home)
            {
                await home.UpdateUIAsync();
            }
        }

        private async void Manager_Connected(object sender)
        {
            await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
            {
                State = $"Connected to {CurrentServer}",
            });
            IsConnecting = false;
            await Dispatcher.InvokeAsync(async () =>
            {
                IsConnected = true;
                IsProcessing = false;
                if (CurrentView is Home home)
                {
                    await home.UpdateUIAsync();
                }
                UpdateFooter();
                StatusFooter.Content = "Status: Connected!";

                ShowSnackbar($"You are now connected to {CurrentServer}");
            });
            connectedAt = DateTime.Now;
        }

        private async void LoginFailed(object sender)
        {
            connectedAt = DateTime.MinValue;
            await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
            {
                State = "Disconnected"
            });
            IsConnecting = false;
            await Dispatcher.InvokeAsync(async () =>
            {
                IsConnected = false;
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
                        if (IsConnected)
                        {
                            MessageBoxResult rsltMessageBox = MessageBox.Show(LightVPN.Resources.Lang.English.NOTIFY_CLOSE, "LightVPN", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                        connectedAt = DateTime.MinValue;
                        ShowSnackbar($"Disconnected!");
                        await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
                        {
                            State = "Disconnected"
                        });
                        StatusFooter.Content = "Status: Disconnected";
                        _manager.Disconnect();
                        IsConnected = false;
                        if (CurrentView is Home home)
                        {
                            await home.UpdateUIAsync();
                        }
                        UpdateFooter();
                        break;
                }
            }
        }
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //Handles Windows Aero maximize changes
            if (this.WindowState == WindowState.Maximized)
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

        public async Task ConnectToServerAsync(string serverId, string location)
        {
            if (_manager.IsConnected) return;
            IsConnecting = true;
            IsProcessing = true;
            CurrentServer = location;
            CurrentServerId = serverId;
            string ovpnFn = string.Empty;
            var files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
            if(!files.Any(x => x.Contains(serverId)))
            {
                await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
                files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "cache"));
            }
            ovpnFn = files.First(x => x.Contains(serverId));

            var existingSettings = await Globals.container.GetInstance<ISettingsManager<Configuration>>().LoadAsync();
            existingSettings.PreviousServer = new PreviousServer { Id = CurrentServerId, Country = CurrentServer };
            
            await Globals.container.GetInstance<ISettingsManager<Configuration>>().SaveAsync(existingSettings);
     
            if (CurrentView is Home home)
            {
                await home.UpdateUIAsync();
            }
            await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
            {
                State = "Connecting"
            });
            ShowSnackbar($"Connecting...");
            StatusFooter.Content = "Status: Connecting";
            ConnectIcon.Kind = PackIconKind.PowerPlugOffOutline;
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
                            MessageBoxResult rsltMessageBox = MessageBox.Show(LightVPN.Resources.Lang.English.NOTIFY_LOGOUT, "LightVPN", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

        public async void UpdateFooter()
        {
            //ExternalAddress = await Globals.container.GetInstance<IHttp>().GetExternalAddressAsync();
            //IPFooter.Content = $"Click to show IP";
            if (IsConnected)
            {
                StatusFooter.Content = "Status: Connected";

            }
            else
            {
                StatusFooter.Content = "Status: Disconnected";
            }
            if (IsConnecting)
            {
                StatusFooter.Content = "Status: Connecting";
            }
        }

        private void IPFooter_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IPFooter.Content.ToString() == "Click to show IP")
            {
                IPFooter.Content = $"IP: {ExternalAddress}";
            }
            else
            {
                IPFooter.Content = "Click to show IP";
            }
        }

        private bool ClosingAnimationFinished;

        private void FinishedClosingAnimation(object sender, EventArgs e)
        {
            try
            {
                _manager.Disconnect();
                _manager.Dispose();
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
                this.MinHeight = 460;
                this.MinWidth = 780;
                BeginStoryboard sb = this.FindResource("CloseAnim") as BeginStoryboard;
                sb.Storyboard.Begin();
                e.Cancel = true;
            }
        }
    }
}
