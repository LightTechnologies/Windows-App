namespace LightVPN.Client.Windows.ViewModels
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using Auth.Exceptions;
    using Auth.Interfaces;
    using Auth.Models;
    using Common;
    using MaterialDesignThemes.Wpf;
    using Services.Interfaces;
    using Utils;

    internal sealed class LoginViewModel : WindowViewModel
    {
        public LoginViewModel() : base(false)
        {
        }

        public string UserName
        {
            get => this._userName;
            set
            {
                this._userName = value;
                this.OnPropertyChanged(nameof(LoginViewModel.UserName));
            }
        }

        private string _userName;

        public bool IsAuthenticating
        {
            get => this._isAuthenticating;
            set
            {
                this._isAuthenticating = value;
                this.OnPropertyChanged(nameof(LoginViewModel.IsAuthenticating));
            }
        }

        private bool _isAuthenticating;


        private int _progressInt = -1;

        public int ProgressInt
        {
            get => this._progressInt;

            set
            {
                this._progressInt = value;
                this.OnPropertyChanged(nameof(LoginViewModel.ProgressInt));
            }
        }

        private string _statusText = "SIGN IN";

        public string StatusText
        {
            get => this._statusText;

            set
            {
                this._statusText = value.ToUpper();
                this.OnPropertyChanged(nameof(LoginViewModel.StatusText));
            }
        }

        private bool _isIndeterminate = true;

        public bool IsIndeterminate
        {
            get => this._isIndeterminate;

            set
            {
                this._isIndeterminate = value;
                this.OnPropertyChanged(nameof(LoginViewModel.IsIndeterminate));
            }
        }

        private string _password;

        public string Password
        {
            get => this._password;

            set
            {
                this._password = value;
                this.OnPropertyChanged(nameof(LoginViewModel.Password));
            }
        }

        public ICommand PasswordChangedCommand =>
            new UICommand
            {
                CommandAction = this.ExecChangePassword,
            };

        private void ExecChangePassword(object obj)
        {
            this.Password = ((PasswordBox) obj).Password;
        }

        [NotNull]
        public ICommand LoginCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(this.UserName)) return;

                            this.IsAuthenticating = true;

                            var apiClient = Globals.Container.GetInstance<IApiClient>();

                            Globals.UserName = this.UserName;

                            await apiClient.PostAsync<AuthResponse>("auth", new
                            {
                                username = this.UserName,
                                password = this.Password,
                            }, this.CancellationTokenSource.Token);

                            // Cache OVPN binaries
                            await Globals.Container.GetInstance<ICacheService>().CacheOpenVpnBinariesAsync();

                            // Cache OVPN configs
                            await Globals.Container.GetInstance<ICacheService>().CacheServersAsync();

                            var loginWindow = (LoginWindow) Globals.LoginWindow;

                            var mainWindow = new MainWindow();
                            mainWindow.Show();

                            loginWindow.Close();
                        }
                        catch (UpdateRequiredException e)
                        {
                            await DialogManager.ShowDialogAsync(PackIconKind.Update, "An update is required!",
                                e.Message);
                        }
                        catch (InvalidResponseException e)
                        {
                            await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                                e.Message);
                        }
                        finally
                        {
                            this.IsAuthenticating = false;
                        }
                    },
                    CanExecuteFunc = () => !this.IsAuthenticating,
                };
            }
        }
    }
}
