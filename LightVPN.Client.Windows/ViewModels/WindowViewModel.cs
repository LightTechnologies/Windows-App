namespace LightVPN.Client.Windows.ViewModels
{
    using System.Windows;
    using System.Windows.Input;
    using Debug;
    using Utils;

    internal class WindowViewModel : BaseViewModel
    {
        private readonly bool _canMaximize;

        protected WindowViewModel(bool canMaximize = true)
        {
            this._canMaximize = canMaximize;
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.StateChanged +=
                    (_, _) => this.WindowState = Application.Current.MainWindow.WindowState;
        }

        public WindowViewModel()
        {
            this._canMaximize = true;
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.StateChanged +=
                    (_, _) => this.WindowState = Application.Current.MainWindow.WindowState;
        }

        private WindowState _windowState;

        public WindowState WindowState
        {
            get => this._windowState;
            set
            {
                this._windowState = value;
                this.OnPropertyChanged(nameof(WindowViewModel.WindowState));
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

                        this.WindowState = Application.Current.MainWindow.WindowState;
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                    },
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

                        DebugLogger.Write("lvpn-client-win-chrome-mvvm",
                            "running this monstrosity of a conditional to determine how to toggle max");

                        this.WindowState = Application.Current.MainWindow.WindowState;
                        Application.Current.MainWindow.WindowState =
                            Application.Current.MainWindow is {WindowState: WindowState.Maximized,}
                                ? WindowState.Normal
                                : WindowState.Maximized;
                    },
                    CanExecuteFunc = () => this._canMaximize,
                };
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ => Application.Current.MainWindow?.Close(),
                };
            }
        }
    }
}
