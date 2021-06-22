using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Utils;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class LoginViewModel : WindowViewModel
    {
        public LoginViewModel() : base(false)
        {
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string _userName;

        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set
            {
                _isAuthenticating = value;
                OnPropertyChanged(nameof(IsAuthenticating));
            }
        }

        private bool _isAuthenticating;


        private int _progressInt = -1;

        public int ProgressInt
        {
            get => _progressInt;

            set
            {
                _progressInt = value;
                OnPropertyChanged(nameof(ProgressInt));
            }
        }

        private string _statusText = "SIGN IN";

        public string StatusText
        {
            get => _statusText;

            set
            {
                _statusText = value.ToUpper();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private bool _isIndeterminate = true;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;

            set
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        private string _password;

        public string Password
        {
            get => _password;

            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand PasswordChangedCommand =>
            new UiCommand
            {
                CommandAction = ExecChangePassword
            };

        private void ExecChangePassword(object obj)
        {
            Password = ((PasswordBox)obj).Password;
        }

        [NotNull]
        public ICommand LoginCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async _ =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(UserName)) return;

                            IsAuthenticating = true;

                            var apiClient = Globals.Container.GetInstance<IApiClient>();

                            Globals.UserName = UserName;

                            await apiClient.PostAsync<AuthResponse>("auth", new
                            {
                                username = UserName,
                                password = Password
                            }, CancellationTokenSource.Token);

                            var loginWindow = (LoginWindow)Globals.LoginWindow;

                            var mainWindow = new MainWindow();
                            mainWindow.Show();

                            loginWindow.Close();
                        }
                        catch (UpdateRequiredException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (InvalidResponseException e)
                        {
                            MessageBox.Show(e.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            IsAuthenticating = false;
                        }
                    },
                    CanExecuteFunc = () => !IsAuthenticating
                };
            }
        }
    }
}