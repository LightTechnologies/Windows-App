using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.OpenVPN.Resources;

namespace LightVPN.Client.OpenVPN.Utils
{
    /// <inheritdoc />
    /// <summary>
    ///     Manages everything to do with the OpenVPN management socket
    /// </summary>
    internal sealed class ManagementSocketHandler : IDisposable
    {
        private IPEndPoint _endPoint;
        private Socket _socket;

        private bool IsConnected => _socket.Connected;

        /// <summary>
        ///     The port that the OpenVPN management server is currently listening on
        /// </summary>
        public ushort Port { get; }

        public ManagementSocketHandler(ushort port)
        {
            Port = port;

            _endPoint = CreateEndPoint();
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        ///     Attempts to establish a connection to the OpenVPN management server
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _endPoint = CreateEndPoint();
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await _socket.ConnectAsync(_endPoint, cancellationToken);
        }

        /// <summary>
        ///     Sends a buffer to the OpenVPN management server
        /// </summary>
        /// <param name="buffer">The buffer (will be converted to a byte array)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        private async Task SendDataAsync(string buffer, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) await ConnectAsync(cancellationToken);

            await _socket.SendAsync(Encoding.UTF8.GetBytes(buffer + "\r\n"), SocketFlags.None, cancellationToken);
        }

        /// <summary>
        ///     Sends a SIGTERM to the OpenVPN management server, causing the OpenVPN process to gracefully exit
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task SendShutdownSignalAsync(CancellationToken cancellationToken = default)
        {
            await SendDataAsync(StringTable.OVPN_SHUTDOWN_SIG, cancellationToken);

            Dispose();
        }

        /// <summary>
        ///     Creates a IPEndPoint instance for the socket
        /// </summary>
        /// <returns>The new IPEndPoint instance</returns>
        private IPEndPoint CreateEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
        }

        /// <summary>
        ///     Gets a available port that is not being used by another process
        /// </summary>
        /// <param name="startingPort">The port to start looking at, this will be the first port if it's available</param>
        /// <returns>A port that is not in use by another process</returns>
        public static ushort GetAvailablePort(ushort startingPort)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
            var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
            var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                .Concat(udpListenersEndpoints)
                .Select(e => e.Port);

            return (ushort)Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse)
                .FirstOrDefault();
        }

        public void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Dispose();
        }
    }
}