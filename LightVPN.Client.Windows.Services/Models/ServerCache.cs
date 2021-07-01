using System;
using System.ComponentModel;
using LightVPN.Client.Windows.Common.Models;

namespace LightVPN.Client.Windows.Services.Models
{
    public class ServerCache
    {
        public BindingList<DisplayVpnServer> Servers { get; set; }
        public DateTime LastCache { get; set; }
    }
}
