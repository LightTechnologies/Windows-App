/* --------------------------------------------
 * 
 * Settings view - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: Settings.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Interfaces;
using LightVPN.Auth.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LightVPN.Common.v2.Models;
using LightVPN.Auth.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.Common.v2.Interfaces;
using LightVPN.OpenVPN.Interfaces;
using static LightVPN.Auth.ApiException;
using System.Reflection;
using LightVPN.Common.v2;
using LightVPN.Discord.Interfaces;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public static readonly DependencyProperty IsWorkingProperty =
            DependencyProperty.Register("IsWorking", typeof(bool),
            typeof(Page), new(false));

        private readonly MainWindow _host;

        public bool IsWorking
        {
            get { return (bool)GetValue(IsWorkingProperty); }
            set { SetValue(IsWorkingProperty, value); }
        }

        public static readonly RoutedUICommand RefreshCacheCommand =
            new("Clear",
                                "RefreshCacheCommand",
                                typeof(Settings));

        public static readonly RoutedUICommand HandleCheckChanges =
            new("HandleChecks",
                                "HandleCheckChanges",
                                typeof(Settings));

        public Settings(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            this.CommandBindings.Add(new CommandBinding(RefreshCacheCommand, RefreshCacheCommand_Event));
            this.CommandBindings.Add(new CommandBinding(HandleCheckChanges, HandleCheckChanges_Event));
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            DarkModeCheckbox.IsChecked = settings.DarkMode;
            DiscordRPCCheckBox.IsChecked = settings.DiscordRPC;
            AutoConnectCheckbox.IsChecked = settings.AutoConnect;
            LightVpnHeadlineText.Text = $"LightVPN Windows Client (version {typeof(Startup).Assembly.GetName().Version})";
        }
        private async void HandleCheckChanges_Event(object sender, ExecutedRoutedEventArgs args)
        {
            Globals.container.GetInstance<IThemeUtils>().SwitchTheme(new Theme
            {
                DarkMode = DarkModeCheckbox.IsChecked.Value,
                SecondaryColor = "Default",
                PrimaryColor = "Default"
            });
            if(DiscordRPCCheckBox.IsChecked.Value)
            {
                Globals.container.GetInstance<IDiscordRpc>().Initalize();
                Globals.container.GetInstance<IDiscordRpc>().ClearPresence();
            }
            else
            {
                Globals.container.GetInstance<IDiscordRpc>().Deinitialize();
            }
            await SaveSettingsAsync();
        }

        private async void RefreshCacheCommand_Event(object sender, ExecutedRoutedEventArgs args)
        {
            Dispatcher.Invoke(() => { IsWorking = true; });
            try
            {
                await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
            }
            catch (InvalidResponseException e)
            {
                MessageBox.Show(e.ResponseString);
                throw;
            }
            catch (ApiOfflineException e)
            {
                _host.ShowSnackbar(e.Message);
            }
            Dispatcher.Invoke(() => { IsWorking = false; });
            _host.ShowSnackbar("Rebuilt cache successfully.");
        }

        private async Task SaveSettingsAsync()
        {
            var settingsManager = Globals.container.GetInstance<ISettingsManager<SettingsModel>>();
            var existingSettings = await settingsManager.LoadAsync();
            var settings = new Common.v2.Models.SettingsModel
            {
                AutoConnect = AutoConnectCheckbox.IsChecked.Value,
                DiscordRPC = DiscordRPCCheckBox.IsChecked.Value,
                DarkMode = DarkModeCheckbox.IsChecked.Value,
                PreviousServer = existingSettings.PreviousServer
            };

            await settingsManager.SaveAsync(settings);
        }
    }
}
