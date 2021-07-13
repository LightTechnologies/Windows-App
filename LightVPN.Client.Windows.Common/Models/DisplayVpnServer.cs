namespace LightVPN.Client.Windows.Common.Models
{
    using System.ComponentModel;

    public sealed class DisplayVpnServer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Country
        {
            get => this._country;

            set
            {
                this._country = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.Country));
            }
        }

        public string Flag
        {
            get => this._flag;

            set
            {
                this._flag = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.Flag));
            }
        }

        public string Id
        {
            get => this._id;

            init
            {
                this._id = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.Id));
            }
        }

        public string ServerName
        {
            get => this._displayName;

            init
            {
                this._displayName = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.ServerName));
            }
        }

        public string Status
        {
            get => this._status;

            set
            {
                this._status = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.Status));
            }
        }

        public VpnServerType Type
        {
            get => this._type;

            set
            {
                this._type = value;
                this.OnPropertyChanged(nameof(DisplayVpnServer.Type));
            }
        }

        private string _country { set; get; }

        private string _displayName { set; get; }

        private string _flag { set; get; }

        private string _id { set; get; }

        private string _status { set; get; }

        private VpnServerType _type { set; get; }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
