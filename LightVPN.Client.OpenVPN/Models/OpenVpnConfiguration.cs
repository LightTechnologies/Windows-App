namespace LightVPN.Client.OpenVPN.Models
{
    /// <summary>
    ///     Contains all the information required by the VpnManager class
    /// </summary>
    public sealed class OpenVpnConfiguration
    {
        /// <summary>
        ///     Path to the OpenVPN binary (openvpn.exe)
        /// </summary>
        public string OpenVpnPath { get; set; }

        /// <summary>
        ///     Path to the OpenVPN log file (output from OpenVPN will be written there)
        /// </summary>
        public string OpenVpnLogPath { get; set; }

        /// <summary>
        ///     Path to the 'tapctl.exe' executable. It's a utility to manipulate TAP/TUN adapters (Windows only)
        /// </summary>
        public string TapCtlPath { get; set; }

        /// <summary>
        ///     The friendly name of the TAP adapter OpenVPN will use (Windows only)
        /// </summary>
        public string TapAdapterName { get; set; }
    }
}