using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.Delegates;
using LightVPN.Discord.Interfaces;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.ViewModels.Base;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private bool autoConnect;

        private bool darkMode;

        private bool discordRpc;

        private bool isRefreshingServerCache;

        private bool isReinstallingTap;

        private bool saveWindowSize;

        public static ICommand ViewErrorLogCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        StartNotepadProcess(Globals.ErrorLogPath);
                    }
                };
            }
        }

        public static ICommand ViewOpenVpnLogCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        StartNotepadProcess(Globals.OpenVpnLogPath);
                    }
                };
            }
        }

        public bool AutoConnect
        {
            get { return autoConnect; }

            set
            {
                autoConnect = value;
                OnPropertyChanged(nameof(AutoConnect));
            }
        }

        public bool DarkMode
        {
            get { return darkMode; }

            set
            {
                darkMode = value;

                ThemeUtils.SwitchTheme("Default", "Default", value);

                OnPropertyChanged(nameof(DarkMode));
            }
        }

        public bool DiscordRpc
        {
            get { return discordRpc; }

            set
            {
                try
                {
                    if (value && discordRpc != value)
                    {
                        Globals.Container.GetInstance<IDiscordRpc>().Initialize();
                    }
                    else if (discordRpc != value)
                    {
                        Globals.Container.GetInstance<IDiscordRpc>().Deinitialize();
                    }
                }
                catch (Exception)
                {
                    // Too lazy to ensure it's not already initialized.
                }

                discordRpc = value;

                OnPropertyChanged(nameof(DiscordRpc));
            }
        }

        public ICommand HandleSettingsChangesCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = (args) =>
                    {
                        var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        settings.DarkMode = DarkMode;
                        settings.AutoConnect = AutoConnect;
                        settings.DiscordRpc = DiscordRpc;
                        settings.SizeSaving ??= new();
                        settings.SizeSaving.IsSavingSize = SaveWindowSize;
                        settings.SizeSaving.Height = 420;
                        settings.SizeSaving.Width = 550;

                        Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Save(settings);
                    }
                };
            }
        }

        public bool IsRefreshingServerCache
        {
            get { return isRefreshingServerCache; }

            set
            {
                isRefreshingServerCache = value;
                OnPropertyChanged(nameof(IsRefreshingServerCache));
            }
        }

        public bool IsReinstallingTap
        {
            get { return isReinstallingTap; }

            set
            {
                isReinstallingTap = value;
                OnPropertyChanged(nameof(IsReinstallingTap));
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = (args) =>
                    {
                        var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        DarkMode = settings.DarkMode;
                        AutoConnect = settings.AutoConnect;
                        DiscordRpc = settings.DiscordRpc;

                        // Prevent crashes on older config versions
                        settings.SizeSaving ??= new();
                        Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Save(settings);

                        SaveWindowSize = settings.SizeSaving.IsSavingSize;
                    }
                };
            }
        }

        public ICommand RefreshServerCacheCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = async (args) =>
                    {
                        IsRefreshingServerCache = true;
                        await Globals.Container.GetInstance<IHttp>().CacheConfigsAsync(true);
                        IsRefreshingServerCache = false;
                        MessageBox.Show("The server cache has been refreshed.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Information);
                    },
                    CanExecuteFunc = () => !IsRefreshingServerCache
                };
            }
        }

        public ICommand ReinstallTapCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async (args) =>
                    {
                        IsReinstallingTap = true;
                        await Task.Run(() =>
                        {
                            var instance = Globals.Container.GetInstance<ITapManager>();
                            if (instance.IsAdapterExistant())
                            {
                                instance.RemoveTapAdapter();
                            }
                            instance.CreateTapAdapter();
                        });
                        IsReinstallingTap = false;
                        MessageBox.Show("OpenVPN TAP adapter has been reinstalled.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Information);
                    },
                    CanExecuteFunc = () => !IsReinstallingTap
                };
            }
        }

        public bool SaveWindowSize
        {
            get { return saveWindowSize; }

            set
            {
                saveWindowSize = value;
                OnPropertyChanged(nameof(SaveWindowSize));
            }
        }

        internal static void StartNotepadProcess(string filePath)
        {
            var process = new Process
            {
                StartInfo = new()
                {
                    UseShellExecute = true,
                    FileName = "notepad.exe",
                    Arguments = filePath
                }
            };
            process.Start();
            process.Exited += (sender, e) =>
            {
                process.Dispose();
            };
        }
    }
}