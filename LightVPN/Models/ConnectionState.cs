using LightVPN.Converters;
using System.ComponentModel;

namespace LightVPN.Models
{
    /// <summary>
    /// Contains the connection states for the UI to bind with
    /// </summary>
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