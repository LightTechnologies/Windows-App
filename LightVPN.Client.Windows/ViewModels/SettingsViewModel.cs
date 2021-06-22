using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Discord.Interfaces;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Services.Interfaces;
using LightVPN.Client.Windows.Utils;
using LightVPN.Client.Windows.Views;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class SettingsViewModel : BaseViewModel
    {
        private AppConfiguration _appConfiguration;

        public AppConfiguration AppConfiguration
        {
            get => _appConfiguration;
            set
            {
                _appConfiguration = value;
                OnPropertyChanged(nameof(AppConfiguration));
            }
        }

        private bool _isReinstallingAdapter;

        public bool IsReinstallingAdapter
        {
            get => _isReinstallingAdapter;
            set
            {
                _isReinstallingAdapter = value;
                OnPropertyChanged(nameof(IsReinstallingAdapter));
            }
        }

        private bool _isRefreshingCache;

        public bool IsRefreshingCache
        {
            get => _isRefreshingCache;
            set
            {
                _isRefreshingCache = value;
                OnPropertyChanged(nameof(IsRefreshingCache));
            }
        }

        public string VersionString { get; set; } =
            $"{(Globals.IsBeta ? "beta" : "stable")} {Assembly.GetEntryAssembly()?.GetName().Version} {HostVersion.GetOsVersion()}";

        public ICommand HandleConfigChanges
        {
            get
            {
                return new UiCommand()
                {
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                        manager.Write(AppConfiguration);

                        var discordClient = Globals.Container.GetInstance<IDiscordRp>();
                        if (AppConfiguration.IsDiscordRpcEnabled)
                            discordClient.Initialize();
                        else
                            discordClient.Deinitialize();

                        ThemeManager.SwitchTheme(ThemeColor.Default,
                            AppConfiguration.IsDarkModeEnabled ? BackgroundMode.Dark : BackgroundMode.Light);
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
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                        AppConfiguration = manager.Read();

                        if (AppConfiguration is not null) return;

                        AppConfiguration = new AppConfiguration();
                        manager.Write(AppConfiguration);
                    }
                };
            }
        }

        public ICommand RefreshCacheCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            IsRefreshingCache = true;

                            await Globals.Container.GetInstance<IOpenVpnService>().CacheServersAsync(true);

                            MessageBox.Show("Cache has been cleared!", "LightVPN", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        catch (InvalidResponseException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        finally
                        {
                            IsRefreshingCache = false;
                        }
                    }
                };
            }
        }

        public ICommand BackCommand
        {
            get
            {
                return new UiCommand()
                {
                    CommandAction = _ =>
                    {
                        var mainWindow = (MainWindow)Application.Current.MainWindow;
                        mainWindow?.LoadView(new MainView());
                    }
                };
            }
        }

        public ICommand ReinstallTapAdapterCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            IsReinstallingAdapter = true;

                            var vpnManager = Globals.Container.GetInstance<IVpnManager>();
                            if (await vpnManager.TapManager?.IsAdapterExistantAsync())
                                await vpnManager.TapManager?.RemoveTapAdapterAsync();

                            await vpnManager.TapManager?.InstallTapAdapterAsync();
                        }
                        catch (InvalidOperationException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        finally
                        {
                            IsReinstallingAdapter = false;
                        }
                    }
                };
            }
        }
    }
}