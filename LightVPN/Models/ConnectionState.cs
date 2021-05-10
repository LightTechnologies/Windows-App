using LightVPN.Converters;
using System.ComponentModel;

namespace LightVPN.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ConnectionState
    {
        [Description("Connected!")]
        Connected,

        [Description("Connecting...")]
        Connecting,

        [Description("Disconnecting...")]
        Disconnecting,

        [Description("Disconnected!")]
        Disconnected
    }
}