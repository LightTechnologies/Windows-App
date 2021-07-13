namespace LightVPN.Client.Windows.Models
{
    using System.ComponentModel;
    using Converters;

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum ConnectionState
    {
        [Description("Disconnected")] Disconnected,
        [Description("Disconnecting...")] Disconnecting,
        [Description("Connecting...")] Connecting,
        [Description("Connected!")] Connected,
    }
}
