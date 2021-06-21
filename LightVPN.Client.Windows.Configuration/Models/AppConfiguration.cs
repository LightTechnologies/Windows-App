namespace LightVPN.Client.Windows.Configuration.Models
{
    public class AppConfiguration
    {
        public AppLastServer LastServer { get; set; }
        public ThemeColor Theme { get; set; }
        public bool IsAutoConnectEnabled { get; set; }
        public bool IsDiscordRpcEnabled { get; set; }
        public bool IsKillSwitchEnabled { get; set; }
        public AppSizeSaving SizeSaving { get; set; }
    }
}