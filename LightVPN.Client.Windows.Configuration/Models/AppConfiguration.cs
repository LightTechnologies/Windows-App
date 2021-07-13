using System.ComponentModel;

namespace LightVPN.Client.Windows.Configuration.Models
{
    using System;
    using System.Reflection;

    public sealed class AppConfiguration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AppLastServer _lastServer;

        public AppLastServer LastServer
        {
            get => this._lastServer;
            set
            {
                this._lastServer = value;
                this.OnPropertyChanged(nameof(AppConfiguration.LastServer));
            }
        }

        private ThemeColor _theme;

        public ThemeColor Theme
        {
            get => this._theme;
            set
            {
                this._theme = value;
                this.OnPropertyChanged(nameof(AppConfiguration.Theme));
            }
        }

        private bool _isAutoConnectEnabled;

        public bool IsAutoConnectEnabled
        {
            get => this._isAutoConnectEnabled;
            set
            {
                this._isAutoConnectEnabled = value;
                this.OnPropertyChanged(nameof(AppConfiguration.IsAutoConnectEnabled));
            }
        }

        private bool _isDiscordRpcEnabled = true;

        public bool IsDiscordRpcEnabled
        {
            get => this._isDiscordRpcEnabled;
            set
            {
                this._isDiscordRpcEnabled = value;
                this.OnPropertyChanged(nameof(AppConfiguration.IsDiscordRpcEnabled));
            }
        }

        private bool _isKillSwitchEnabled;

        public bool IsKillSwitchEnabled
        {
            get => this._isKillSwitchEnabled;
            set
            {
                this._isKillSwitchEnabled = value;
                this.OnPropertyChanged(nameof(AppConfiguration.IsKillSwitchEnabled));
            }
        }

        private bool _isDarkModeEnabled;

        public bool IsDarkModeEnabled
        {
            get => this._isDarkModeEnabled;
            set
            {
                this._isDarkModeEnabled = value;
                this.OnPropertyChanged(nameof(AppConfiguration.IsDarkModeEnabled));
            }
        }

        private AppSizeSaving _sizeSaving;

        public AppSizeSaving SizeSaving
        {
            get => this._sizeSaving;
            set
            {
                this._sizeSaving = value;
                this.OnPropertyChanged(nameof(AppConfiguration.SizeSaving));
            }
        }

        public bool IsFirstRun { get; set; } = true;

        public Version ConfigVersion { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
