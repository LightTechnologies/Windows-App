namespace LightVPN.Client.OpenVPN.Models
{
    public sealed class OpenVpnConfiguration
    {
        public string OpenVpnPath { get; set; }
        public string OpenVpnLogPath { get; set; }
        public string TapCtlPath { get; set; }
        public string TapAdapterName { get; set; }
    }
}