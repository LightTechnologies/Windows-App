using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.Discord.Interfaces;
using LightVPN.Interfaces;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool autoConnect;
        public bool AutoConnect
        {
            get { return autoConnect; }
            set
            {
                autoConnect = value;
                OnPropertyChanged(nameof(AutoConnect));
            }
        }

        private bool isRefreshingServerCache;
        public bool IsRefreshingServerCache
        {
            get { return isRefreshingServerCache; }
            set
            {
                isRefreshingServerCache = value;
                OnPropertyChanged(nameof(IsRefreshingServerCache));
            }
        }

        private bool isReinstallingTap;
        public bool IsReinstallingTap
        {
            get { return isReinstallingTap; }
            set
            {
                isReinstallingTap = value;
                OnPropertyChanged(nameof(IsReinstallingTap));
            }
        }

        private bool discordRpc;
        public bool DiscordRpc
        {
            get { return discordRpc; }
            set
            {
                try
                {
                    if (value && discordRpc != value)
                    {
                        Globals.container.GetInstance<IDiscordRpc>().Initialize();
                    }
                    else if (discordRpc != value)
                    {
                        Globals.container.GetInstance<IDiscordRpc>().Deinitialize();
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

        private bool darkMode;
        public bool DarkMode
        {
            get { return darkMode; }
            set
            {
                darkMode = value;

                Globals.container.GetInstance<IThemeUtils>().SwitchTheme(new Auth.Models.Theme
                {
                    DarkMode = value,
                    SecondaryColor = "Default",
                    PrimaryColor = "Default"
                });

                OnPropertyChanged(nameof(DarkMode));
            }
        }

        public ICommand RefreshServerCacheCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = async () =>
                    {
                        IsRefreshingServerCache = true;
                        await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
                        IsRefreshingServerCache = false;
                        MessageBox.Show("The server cache has been refreshed.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Information);
                    },
                    CanExecuteFunc = () => !IsRefreshingServerCache
                };
            }
        }

        internal void StartNotepadProcess(string filePath)
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

        public ICommand ViewErrorLogCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = () =>
                    {
                        StartNotepadProcess(Globals.ErrorLogPath);
                    }
                };
            }
        }

        public ICommand ViewOpenVpnLogCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = () =>
                    {
                        StartNotepadProcess(Globals.OpenVpnLogPath);
                    }
                };
            }
        }


        public ICommand ReinstallTapCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = async () =>
                    {
                        IsReinstallingTap = true;
                        await Task.Run(() =>
                        {
                            var instance = Globals.container.GetInstance<ITapManager>();
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


        public ICommand HandleSettingsChangesCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = () =>
                    {
                        var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        settings.DarkMode = DarkMode;
                        settings.AutoConnect = AutoConnect;
                        settings.DiscordRpc = DiscordRpc;

                        Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Save(settings);
                    }
                };
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = () =>
                    {
                        var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                        DarkMode = settings.DarkMode;
                        AutoConnect = settings.AutoConnect;
                        DiscordRpc = settings.DiscordRpc;
                    }
                };
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}