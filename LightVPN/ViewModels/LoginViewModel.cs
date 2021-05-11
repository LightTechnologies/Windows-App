using Exceptionless;
using LightVPN.Auth;
using LightVPN.Auth.Exceptions;
using LightVPN.Auth.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.Models;
using LightVPN.Delegates;
using LightVPN.Discord.Interfaces;
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings;
using LightVPN.Settings.Exceptions;
using LightVPN.Settings.Interfaces;
using LightVPN.ViewModels.Base;
using LightVPN.Windows;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class LoginViewModel : BaseViewModel, IDisposable
    {
        private bool isAuthenticating;

        private bool isIndeterminate = true;

        private FileLoggerBase logger = new ErrorLogger();

        private string password;

        private int progressInt = -1;

        private string statusText = "SIGN IN";

        private string userName;

        public bool IsAuthenticating
        {
            get { return isAuthenticating; }

            set
            {
                isAuthenticating = value;
                OnPropertyChanged(nameof(IsAuthenticating));
            }
        }

        public bool IsIndeterminate
        {
            get { return isIndeterminate; }

            set
            {
                isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = async (args) =>
                    {
                        if (File.Exists(Globals.AuthPath))
                        {
                            try
                            {
                                var auth = JsonConvert.DeserializeObject<AuthFile>(Encryption.Decrypt(File.ReadAllText(Globals.AuthPath)));
                                UserName = auth.Username;
                                Password = auth.Password;
                                if (auth.SessionId != default)
                                {
                                    await ProcessLoginAsync(true, auth.SessionId);
                                }
                            }
                            catch (CorruptedAuthSettingsException ex)
                            {
                                File.Delete(Globals.AuthPath);
                                MessageBox.Show(ex.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }
                };
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CanExecuteFunc = () => !IsAuthenticating,
                    CommandAction = async (args) =>
                    {
                        await ProcessLoginAsync(false, default);
                    }
                };
            }
        }

        public string Password
        {
            get { return password; }

            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand PasswordChangedCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = (args) =>
                    {
                        ExecChangePassword(args);
                    }
                };
            }
        }

        public int ProgressInt
        {
            get { return progressInt; }

            set
            {
                progressInt = value;
                OnPropertyChanged(nameof(ProgressInt));
            }
        }

        public string StatusText
        {
            get { return statusText; }

            set
            {
                statusText = value.ToUpper();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string UserName
        {
            get { return userName; }

            set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public void Dispose()
        {
            logger = null;
            Password = null;
            UserName = null;
            ProgressInt = default;
            StatusText = null;
            IsIndeterminate = default;
            Startup.LoginWindow = null;
            GC.SuppressFinalize(this);
        }

        private static void OpenMainWindow()
        {
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
            Startup.LoginWindow.Close();
        }

        private void ExecChangePassword(object obj)
        {
            Password = ((PasswordBox)obj).Password;
        }

        private async Task ProcessLoginAsync(bool isSessionAuth = false, Guid sessionId = default)
        {
            if (!isSessionAuth)
            {
                if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password)) return;
            }

            IsAuthenticating = true;
            SetProgressIndeterminate();
            StatusText = "Signing in...";
            try
            {
                AuthResponse authResponse = new();
                if (isSessionAuth)
                {
                    var sessionResponse = await Globals.container.GetInstance<IHttp>().ValidateSessionAsync(UserName, sessionId);
                    if (!sessionResponse)
                    {
                        MessageBox.Show("Your session has been closed or is invalid, please sign back in.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    authResponse = await Globals.container.GetInstance<IHttp>().LoginAsync(UserName, Password);
                }

                var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();

                if (!Globals.container.GetInstance<ITapManager>().CheckDriverExists())
                {
                    StatusText = "Fetching drivers...";
                    IsIndeterminate = false;
                    await Globals.container.GetInstance<IHttp>().FetchOpenVpnDriversAsync();
                    ProgressInt = 50;
                    await Globals.container.GetInstance<ITapManager>().InstallDriverAsync();
                    SetProgressIndeterminate();
                }

                if (!isSessionAuth)
                {
                    await File.WriteAllTextAsync(Globals.AuthPath, Encryption.Encrypt(JsonConvert.SerializeObject(new AuthFile { Username = UserName, Password = Password, SessionId = authResponse.SessionId })));
                }

                if (!Http.IsConfigsCached())
                {
                    StatusText = "Fetching cache...";

                    await Globals.container.GetInstance<IHttp>().CacheConfigsAsync();
                }
                if (!Http.HasOpenVpn())
                {
                    StatusText = "Fetching OpenVPN...";

                    await Globals.container.GetInstance<IHttp>().GetOpenVpnBinariesAsync();
                }

                if (!Globals.container.GetInstance<ITapManager>().IsAdapterExistant())
                {
                    StatusText = "Installing VPN adapter...";

                    Globals.container.GetInstance<ITapManager>().CreateTapAdapter();
                }

                StatusText = "Loading...";

                if (settings.DiscordRpc)
                {
                    Globals.container.GetInstance<IDiscordRpc>().Initialize();
                }

                // After login is successful
                OpenMainWindow();
            }
            catch (ClientUpdateRequired)
            {
                StatusText = "Fetching updates...";
                await Globals.container.GetInstance<IHttp>().GetUpdatesAsync();
            }
            catch (InvalidResponseException e)
            {
                MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);

                await logger.WriteAsync(e.ResponseString);

                if (e.InnerException != null)
                {
                    logger.Write(e.InnerException.Message);
                    e.ToExceptionless().SetUserIdentity(UserName).AddObject(StatusText, "SignInText").Submit();
                }
            }
            catch (RatelimitedException e)
            {
                MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (HttpRequestException e)
            {
                switch (e.Message)
                {
                    case "The SSL connection could not be established, see inner exception.":
                        MessageBox.Show("API certificate check failed.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;

                    default:
                        await logger.WriteAsync(e.ToString() + "\n" + StatusText); e.ToExceptionless().SetUserIdentity(UserName).AddObject(StatusText, "SignInText").Submit();
                        MessageBox.Show("Failed to send HTTP request to the LightVPN API.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            }
            catch (Exception e)
            {
                await logger.WriteAsync(e.ToString());

                MessageBox.Show("Something went wrong, check the error log for more info.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsAuthenticating = false;
                StatusText = "Sign in";
            }
        }

        private void SetProgressIndeterminate()
        {
            ProgressInt = -1;
            IsIndeterminate = true;
        }
    }
}