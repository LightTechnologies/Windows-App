using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN.Utils
{
    public static class SocketUtils
    {
        /// <summary>
        /// Gets a random port that is not currently in use by another application
        /// </summary>
        /// <param name="startingPort">The port that we should start from</param>
        /// <returns>Random port that is available to bind to</returns>
        public static ushort GetAvailablePort(ushort startingPort)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
            var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
            var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                                                 .Concat(udpListenersEndpoints)
                                                 .Select(e => e.Port);

            return (ushort)Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).FirstOrDefault();
        }

        public static EndPoint GetEndPoint(ushort port) => new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

        public static Socket GetSocket(EndPoint endPoint)
        {
            var sock = new Socket(endPoint.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

            return sock;
        }

        public static string ReceiveBuffer(this Socket socket)
        {
            byte[] bytes = new byte[1024];

            socket.Receive(bytes, SocketFlags.None);

            return Encoding.UTF8.GetString(bytes);
        }

        public static void SendBuffer(this Socket socket, byte[] buffer)
        {
            /* \r\n is required because OpenVPN Management Interface says so (and it likes legacy). */
            socket.Send(buffer, SocketFlags.None);
        }
    }
}