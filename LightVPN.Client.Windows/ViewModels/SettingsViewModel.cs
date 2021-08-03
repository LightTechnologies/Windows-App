namespace LightVPN.Client.Windows.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Auth.Exceptions;
    using Common;
    using Common.Utils;
    using Configuration.Interfaces;
    using Configuration.Models;
    using Debug;
    using Discord.Interfaces;
    using MaterialDesignThemes.Wpf;
    using Models;
    using OpenVPN.Interfaces;
    using Services.Interfaces;
    using Utils;
    using Views;

    internal sealed class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Globals.Container.GetInstance<IVpnManager>().TapManager.OnErrorReceived += async (_, args) =>
                await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
                    PackIconKind.ErrorOutline, "Something went wrong...",
                    args.Exception.Message));

            Globals.Container.GetInstance<IVpnManager>().TapManager.OnSuccess += async _ =>
                await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
                    PackIconKind.CheckCircleOutline, "Success!",
                    "The VPN adapter has been reinstalled."));
        }

        private AppConfiguration _appConfiguration;

        public AppConfiguration AppConfiguration
        {
            get => this._appConfiguration;
            set
            {
                this._appConfiguration = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.AppConfiguration));
            }
        }

        private bool _isLogProcessOpen;

        public bool IsLogProcessOpen
        {
            get => this._isLogProcessOpen;
            set
            {
                this._isLogProcessOpen = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.IsLogProcessOpen));
            }
        }

        private bool _isAppLogProcessOpen;

        public bool IsAppLogProcessOpen
        {
            get => this._isAppLogProcessOpen;
            set
            {
                this._isAppLogProcessOpen = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.IsAppLogProcessOpen));
            }
        }

        private bool _isReinstallingAdapter;

        public bool IsReinstallingAdapter
        {
            get => this._isReinstallingAdapter;
            set
            {
                this._isReinstallingAdapter = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.IsReinstallingAdapter));
            }
        }

        private bool _isRunningOnStartup = StartupHelper.IsRunningOnStartup();

        public bool IsRunningOnStartup
        {
            get => this._isRunningOnStartup;
            set
            {
                this._isRunningOnStartup = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.IsRunningOnStartup));
            }
        }

        private bool _isRefreshingCache;

        public bool IsRefreshingCache
        {
            get => this._isRefreshingCache;
            set
            {
                this._isRefreshingCache = value;
                this.OnPropertyChanged(nameof(SettingsViewModel.IsRefreshingCache));
            }
        }

        public string VersionString { get; set; } =
            $"{"preview 3" /*(Globals.IsBeta ? "beta" : "stable")*/} {Assembly.GetEntryAssembly()?.GetName().Version} {HostVersion.GetOsVersion()}";

        public ICommand HandleConfigChanges
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                        manager.Write(this.AppConfiguration);

                        var discordClient = Globals.Container.GetInstance<IDiscordRp>();
                        if (this.AppConfiguration.IsDiscordRpcEnabled)
                        {
                            discordClient.Initialize();
                            if (Globals.Container.GetInstance<IVpnManager>().IsConnected) discordClient.UpdateState($"Connected!");
                            if (Globals.Container.GetInstance<IVpnManager>().IsConnecting) discordClient.UpdateState($"Connecting...");
                        }
                        else
                            discordClient.Deinitialise();

                        if (this.IsRunningOnStartup)
                            StartupHelper.EnableRunOnStartup(Process.GetCurrentProcess().MainModule?.FileName);
                        else
                            StartupHelper.DisableRunOnStartup();

                        ThemeManager.SwitchTheme(ThemeColor.Default,
                            this.AppConfiguration.IsDarkModeEnabled ? BackgroundMode.Dark : BackgroundMode.Light);
                    },
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
                        this.AppConfiguration = manager.Read();

                        if (this.AppConfiguration is not null) return;

                        this.IsRunningOnStartup = StartupHelper.IsRunningOnStartup();

                        this.AppConfiguration = new AppConfiguration();
                        manager.Write(this.AppConfiguration);
                    },
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
                            this.IsRefreshingCache = true;

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
                            this.IsRefreshingCache = false;
                        }
                    },
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
                        this.IsPlayingAnimation = true;
                        var mainWindow = (MainWindow) Application.Current.MainWindow;
                        mainWindow?.LoadView(new MainView());
                    },
                    CanExecuteFunc = () => !this.IsPlayingAnimation,
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
                                Arguments = Globals.OpenVpnLogPath,
                            },
                        };

                        notepadProcess.Start();
                        this.IsLogProcessOpen = true;

                        await notepadProcess.WaitForExitAsync();

                        this.IsLogProcessOpen = false;
                        notepadProcess.Dispose();
                    },
                    CanExecuteFunc = () => !this.IsLogProcessOpen,
                };
            }
        }

        public ICommand AppLogsCommand
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
                                Arguments = DebugLogger.DebugLogLocation,
                            },
                        };

                        notepadProcess.Start();
                        this.IsAppLogProcessOpen = true;

                        await notepadProcess.WaitForExitAsync();

                        this.IsAppLogProcessOpen = false;
                        notepadProcess.Dispose();
                    },
                    CanExecuteFunc = () => !this.IsAppLogProcessOpen,
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
                            this.IsReinstallingAdapter = true;

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
                            this.IsReinstallingAdapter = false;
                        }
                    },
                };
            }
        }
    }
}
