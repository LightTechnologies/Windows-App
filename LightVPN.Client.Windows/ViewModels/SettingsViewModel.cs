using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Discord.Interfaces;
using LightVPN.Client.OpenVPN.EventArgs;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.OpenVPN.Utils;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Services.Interfaces;
using LightVPN.Client.Windows.Utils;
using LightVPN.Client.Windows.Views;
using MaterialDesignThemes.Wpf;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Globals.Container.GetInstance<IVpnManager>().TapManager.OnErrorReceived += async (_, args) =>
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                        args.Exception.Message);
                });
            };

            Globals.Container.GetInstance<IVpnManager>().TapManager.OnSuccess += async (_) =>
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await DialogManager.ShowDialogAsync(PackIconKind.CheckCircleOutline, "Success!",
                        "The VPN adapter has been reinstalled.");
                });
            };
        }

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

        private bool _isLogProcessOpen;

        public bool IsLogProcessOpen
        {
            get => _isLogProcessOpen;
            set
            {
                _isLogProcessOpen = value;
                OnPropertyChanged(nameof(IsLogProcessOpen));
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

        private bool _isRunningOnStartup = StartupHelper.IsRunningOnStartup();

        public bool IsRunningOnStartup
        {
            get => _isRunningOnStartup;
            set
            {
                _isRunningOnStartup = value;
                OnPropertyChanged(nameof(IsRunningOnStartup));
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
            $"{"preview 2"/*(Globals.IsBeta ? "beta" : "stable")*/} {Assembly.GetEntryAssembly()?.GetName().Version} {HostVersion.GetOsVersion()}";

        public ICommand HandleConfigChanges
        {
            get
            {
                return new UICommand
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

                        if (IsRunningOnStartup)
                            StartupHelper.EnableRunOnStartup(Process.GetCurrentProcess().MainModule.FileName);
                        else
                            StartupHelper.DisableRunOnStartup();

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
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                        AppConfiguration = manager.Read();

                        if (AppConfiguration is not null) return;

                        IsRunningOnStartup = StartupHelper.IsRunningOnStartup();

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
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            IsRefreshingCache = true;

                            await Globals.Container.GetInstance<ICacheService>().CacheServersAsync(true);

                            await DialogManager.ShowDialogAsync(PackIconKind.CheckCircleOutline, "Success!",
                                "Cache has been cleared.");
                        }
                        catch (InvalidResponseException e)
                        {
                            await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
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
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        IsPlayingAnimation = true;
                        var mainWindow = (MainWindow) Application.Current.MainWindow;
                        mainWindow?.LoadView(new MainView());
                    },
                    CanExecuteFunc = () => !IsPlayingAnimation
                };
            }
        }

        public ICommand VpnLogsCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        var notepadProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "notepad.exe",
                                UseShellExecute = true,
                                Arguments = Globals.OpenVpnLogPath
                            }
                        };

                        notepadProcess.Start();
                        IsLogProcessOpen = true;

                        await notepadProcess.WaitForExitAsync();

                        IsLogProcessOpen = false;
                        notepadProcess.Dispose();
                    }
                };
            }
        }

        public ICommand ReinstallTapAdapterCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            IsReinstallingAdapter = true;

                            var vpnManager = Globals.Container.GetInstance<IVpnManager>();
                            if (await vpnManager.TapManager.IsAdapterExistentAsync())
                                await vpnManager.TapManager.RemoveTapAdapterAsync();

                            await vpnManager.TapManager.InstallTapAdapterAsync();
                        }
                        catch (InvalidOperationException e)
                        {
                            await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
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
