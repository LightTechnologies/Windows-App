namespace LightVPN.Client.Windows.Configuration.Models
{
    public sealed class AppSizeSaving
    {
        public bool IsSavingSizeEnabled { get; set; } = true;
        public uint Width { get; set; }
        public uint Height { get; set; }
    }
}