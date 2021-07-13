namespace LightVPN.Client.Windows.Services.Models
{
    using System;
    using System.ComponentModel;
    using Common.Models;

    public sealed class ServerCache
    {
        public BindingList<DisplayVpnServer> Servers { get; set; }
        public DateTime LastCache { get; set; }
    }
}
