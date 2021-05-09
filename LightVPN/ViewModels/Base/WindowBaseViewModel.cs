using LightVPN.Common.Models;
using LightVPN.Delegates;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels.Base
{
    public abstract class WindowBaseViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly bool _isLoginWindow;

        public WindowBaseViewModel(bool isLoginWindow)
        {
            _isLoginWindow = isLoginWindow;
        }

        public ICommand ExitWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        Application.Current.Shutdown();
                    }
                };
            }
        }

        public ICommand HideWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        Globals.IsMinimizedToTray = true;
                        Application.Current.MainWindow.Hide();
                    },
                    CanExecuteFunc = () => Globals.IsMinimizedToTray == false
                };
            }
        }

        public ICommand MinimizeWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        if (_isLoginWindow && Startup.LoginWindow is not null)
                        {
                            Startup.LoginWindow.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        }
                    }
                };
            }
        }

        public ICommand ToggleMaximizeWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        if (_isLoginWindow && Startup.LoginWindow is not null)
                        {
                            if (Startup.LoginWindow.WindowState == WindowState.Maximized)
                            {
                                Startup.LoginWindow.WindowState = WindowState.Normal;
                            }
                            else
                            {
                                Startup.LoginWindow.WindowState = WindowState.Maximized;
                            }
                        }
                        else
                        {
                            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                            {
                                Application.Current.MainWindow.WindowState = WindowState.Normal;
                            }
                            else
                            {
                                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                            }
                        }
                    }
                };
            }
        }
    }
}