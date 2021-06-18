using System;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Windows.Common.Utils;

namespace LightVPN.Client.Windows.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        public LoginViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
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

        [NotNull]
        public ICommand LoginCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = async args =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(UserName)) return;

                            IsAuthenticating = true;

                            var authResponse = await _apiClient.PostAsync<AuthResponse>("auth", new
                            {
                                username = UserName,
                                password = "password"
                            }, CancellationTokenSource.Token);

                            MessageBox.Show(authResponse.SessionId);
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