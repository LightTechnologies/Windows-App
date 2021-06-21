using System.ComponentModel;
using LightVPN.Client.Windows.Converters;

namespace LightVPN.Client.Windows.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum ConnectionState
    {
        [Description("Disconnected")] Disconnected,
        [Description("Connecting...")] Connecting,
        [Description("Connected!")] Connected
    }
}