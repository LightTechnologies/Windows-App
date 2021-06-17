using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.OpenVPN.Resources;

namespace LightVpn.Client.OpenVpn.Utils
{
    internal sealed class ManagementSocketHandler : IDisposable
    {
        private IPEndPoint _endPoint;
        private Socket _socket;

        public bool IsConnected => _socket.Connected;
        public bool IsDisposed { get; private set; }
        public ushort Port { get; }

        public ManagementSocketHandler(ushort port)
        {
            Port = port;

            _endPoint = CreateEndPoint();
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _endPoint ??= CreateEndPoint();
            _socket ??= new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await _socket.ConnectAsync(_endPoint, cancellationToken);
        }

        private async Task SendDataAsync(string buffer,CancellationToken cancellationToken = default)
        {
            if (!IsConnected) await ConnectAsync(cancellationToken);

            await _socket.SendAsync(Encoding.UTF8.GetBytes(buffer), SocketFlags.None, cancellationToken);
        }

        public async Task SendShutdownSignalAsync(CancellationToken cancellationToken = default)
        {
            await SendDataAsync(StringTable.OVPN_SHUTDOWN_SIG, cancellationToken);

            Dispose();
        }

        private IPEndPoint CreateEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
        }

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

            IsDisposed = true;
        }
    }
}