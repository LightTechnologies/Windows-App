/* --------------------------------------------
 *
 * Servers data model - Model
 * Copyright (C) Light Technologies LLC
 *
 * File: ServersGrid.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.Common.Models;
using System.ComponentModel;
using System.Net;

namespace LightVPN.Auth.Models
{
    public struct ServersGrid : INotifyPropertyChanged
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

        public IPAddress IpAddress
        {
            get
            {
                return _ipAddress;
            }

            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public string Server
        {
            get
            {
                return _server;
            }

            set
            {
                _server = value;
                OnPropertyChanged(nameof(Server));
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

        public ServerType Type
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

        private IPAddress _ipAddress { set; get; }

        private string _server { set; get; }

        private ServerType _type { set; get; }

        private void OnPropertyChanged(string propertyName)
        {
            var saved = PropertyChanged;
            if (saved != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                saved(this, e);
            }
        }
    }
}