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
    /// <summary>
    /// View model for the settings view
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        private bool autoConnect;

        private bool darkMode;

        private bool discordRpc;

        private bool isRefreshingServerCache;

        private bool isReinstallingTap;

        private bool saveWindowSize;
        /// <summary>
        /// Command that opens the error log in notepad
        /// </summary>
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

        /// <summary>
        /// Command that opens the OpenVPN log in notepad
        /// </summary>
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
        /// <summary>
        /// Binded boolean value that represents the auto connect value in settings
        /// </summary>
        public bool AutoConnect
        {
            get { return autoConnect; }

            set
            {
                autoConnect = value;
                OnPropertyChanged(nameof(AutoConnect));
            }
        }
        /// <summary>
        /// Binded boolean value that represents the dark mode value in settings
        /// </summary>
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
        /// <summary>
        /// Binded boolean value that represents the Discord RPC value in settings
        /// </summary>
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
        /// <summary>
        /// Handles the settings changes, this is called for every toggle switch change
        /// </summary>
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
        /// <summary>
        /// Tells the UI if the server cache is currently being refreshed, this controls the progress bar and the state of the button
        /// </summary>
        public bool IsRefreshingServerCache
        {
            get { return isRefreshingServerCache; }

            set
            {
                isRefreshingServerCache = value;
                OnPropertyChanged(nameof(IsRefreshingServerCache));
            }
        }
        /// <summary>
        /// Tells the UI if the TAP adapter is being reinstalled, this controls the progrss bar and the state of the button
        /// </summary>
        public bool IsReinstallingTap
        {
            get { return isReinstallingTap; }

            set
            {
                isReinstallingTap = value;
                OnPropertyChanged(nameof(IsReinstallingTap));
            }
        }
        /// <summary>
        /// Command that handles the views initial loading
        /// </summary>
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
        /// <summary>
        /// Refreshes the VPN server cache
        /// </summary>
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
        /// <summary>
        /// Reinstalls the OpenVPN TAP adapter
        /// </summary>
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
        /// <summary>
        /// Binded boolean value that represents the window saving value in settings
        /// </summary>
        public bool SaveWindowSize
        {
            get { return saveWindowSize; }

            set
            {
                saveWindowSize = value;
                OnPropertyChanged(nameof(SaveWindowSize));
            }
        }
        /// <summary>
        /// Starts a notepad process
        /// </summary>
        /// <param name="filePath">The file notepad should open</param>
        private static void StartNotepadProcess(string filePath)
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