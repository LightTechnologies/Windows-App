using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Windows.Common.Utils;

namespace LightVPN.Client.Windows.ViewModels
{
    internal class WindowViewModel : BaseViewModel
    {
        public WindowViewModel()
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.StateChanged += (sender, args) => WindowState = Application.Current.MainWindow.WindowState;
        }

        private WindowState _windowState;

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged(nameof(WindowState));
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = _ =>
                    {
                        if (Application.Current.MainWindow == null) return;

                        WindowState = Application.Current.MainWindow.WindowState;
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                    }
                };
            }
        }

        public ICommand ToggleMaxCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = _ =>
                    {
                        if (Application.Current.MainWindow == null) return;

                        WindowState = Application.Current.MainWindow.WindowState;
                        Application.Current.MainWindow.WindowState =
                            Application.Current.MainWindow is { WindowState: WindowState.Maximized }
                                ? WindowState.Normal
                                : WindowState.Maximized;
                    }
                };
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new UiCommand
                {
                    CommandAction = _ => Application.Current.MainWindow?.Close()
                };
            }
        }
    }
}