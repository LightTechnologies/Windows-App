using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Debug;
using LightVPN.Client.Windows.Utils;

namespace LightVPN.Client.Windows.ViewModels
{
    internal class WindowViewModel : BaseViewModel
    {
        private readonly bool _canMaximize;

        protected WindowViewModel(bool canMaximize = true)
        {
            _canMaximize = canMaximize;
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.StateChanged += (_, _) =>
                    WindowState = Application.Current.MainWindow.WindowState;
        }

        public WindowViewModel()
        {
            _canMaximize = true;
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.StateChanged += (_, _) =>
                    WindowState = Application.Current.MainWindow.WindowState;
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
                return new UICommand
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
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        if (Application.Current.MainWindow == null) return;

                        DebugLogger.Write("lvpn-client-win-chrome-mvvm", $"running this monstrosity of a conditional to determine how to toggle max");

                        WindowState = Application.Current.MainWindow.WindowState;
                        Application.Current.MainWindow.WindowState =
                            Application.Current.MainWindow is { WindowState: WindowState.Maximized }
                                ? WindowState.Normal
                                : WindowState.Maximized;
                    },
                    CanExecuteFunc = () => _canMaximize
                };
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ => Application.Current.MainWindow?.Close()
                };
            }
        }
    }
}
