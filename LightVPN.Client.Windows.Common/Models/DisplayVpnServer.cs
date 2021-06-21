using System.ComponentModel;

namespace LightVPN.Client.Windows.Common.Models
{
    public class DisplayVpnServer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Country
        {
            get
            {
                return _country;
            }

            set
            {
                _country = value;
                OnPropertyChanged(nameof(Country));
            }
        }

        public string Flag
        {
            get
            {
                return _flag;
            }

            set
            {
                _flag = value;
                OnPropertyChanged(nameof(Flag));
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string ServerName
        {
            get
            {
                return _displayName;
            }

            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(ServerName));
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public VpnServerType Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
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
            if (PropertyChanged is null) return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
