using System.ComponentModel;

namespace LightVPN.Client.Windows.Configuration.Models
{
    public sealed class AppConfiguration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AppLastServer _lastServer;

        public AppLastServer LastServer
        {
            get => _lastServer;
            set
            {
                _lastServer = value;
                OnPropertyChanged(nameof(LastServer));
            }
        }

        private ThemeColor _theme;

        public ThemeColor Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        private bool _isAutoConnectEnabled;

        public bool IsAutoConnectEnabled
        {
            get => _isAutoConnectEnabled;
            set
            {
                _isAutoConnectEnabled = value;
                OnPropertyChanged(nameof(IsAutoConnectEnabled));
            }
        }

        private bool _isDiscordRpcEnabled = true;

        public bool IsDiscordRpcEnabled
        {
            get => _isDiscordRpcEnabled;
            set
            {
                _isDiscordRpcEnabled = value;
                OnPropertyChanged(nameof(IsDiscordRpcEnabled));
            }
        }

        private bool _isKillSwitchEnabled;

        public bool IsKillSwitchEnabled
        {
            get => _isKillSwitchEnabled;
            set
            {
                _isKillSwitchEnabled = value;
                OnPropertyChanged(nameof(IsKillSwitchEnabled));
            }
        }

        private bool _isDarkModeEnabled;

        public bool IsDarkModeEnabled
        {
            get => _isDarkModeEnabled;
            set
            {
                _isDarkModeEnabled = value;
                OnPropertyChanged(nameof(IsDarkModeEnabled));
            }
        }

        private AppSizeSaving _sizeSaving;

        public AppSizeSaving SizeSaving
        {
            get => _sizeSaving;
            set
            {
                _sizeSaving = value;
                OnPropertyChanged(nameof(SizeSaving));
            }
        }

        public bool IsFirstRun { get; set; } = true;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}