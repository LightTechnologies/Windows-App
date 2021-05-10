/* --------------------------------------------
 *
 * OpenVPN Manager - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: Manager.cs
 *
 * Created: 04-03-21 Toshiro
 *
 * --------------------------------------------
 */

using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using LightVPN.OpenVPN.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN
{
    public class Manager : IManager
    {
        private readonly FileLoggerBase _errorLogger = new ErrorLogger();

        private readonly FileLoggerBase _logger = new OpenVpnLogger();

        private readonly string _ovpnPath;

        private readonly Process _ovpnProcess = new();

        private string _config;

        private EndPoint _managementEp;

        private Socket _managementSocket;

        private ushort _retryCount;

        /// <summary>
        /// Constructs the OpenVPN manager class
        /// </summary>
        /// <param name="openVpnExeFileName">Path to the OpenVPN binary</param>
        /// <param name="tap">Friendly name of the OpenVPN TAP adapter</param>
        public Manager(string openVpnExeFileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe")
        {
            try
            {
                _errorLogger.Write("(Manager/ctor) Cleaning...");
                Process.GetProcessesByName("openvpn").ToList().ForEach(x =>
                {
                    x.Kill();
                });
            }
            catch
            {
            }
            _ovpnPath = openVpnExeFileName;
            IsDisposed = false;
        }

        ~Manager()
        {
            try
            {
                this.Dispose(false);
            }
            catch (Exception)
            {
            }
        }

        public delegate void ConnectedEvent(object sender);

        public delegate void ErrorEvent(object sender, string message);

        public delegate void OutputEvent(object sender, OutputType e, string message);

        public delegate void OutputReceived(object sender, DataReceivedEventArgs e);

        public event ConnectedEvent Connected;

        public event ErrorEvent Error;

        public event OutputReceived OnOutput;

        public event OutputEvent Output;

        /// <summary>
        /// Defines what types of output can be thrown
        /// </summary>
        public enum OutputType
        {
            Normal,

            Connected,

            Error
        }

        public bool IsConnected { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Connects to the specified OpenVPN config file
        /// </summary>
        /// <param name="configpath">Path to the configuration file</param>
        public void Connect(string configpath)
        {
            if (IsConnected) throw new Exception("Already connected to the VPN");
            _config = configpath;
            RunOpenVpnProcess(_ovpnPath);
            IsConnected = true;
        }

        /// <summary>
        /// Disconnects from any VPN currently connected
        /// </summary>
        public async void Disconnect()
        {
            await ShutdownManagementServerAsync();

            _ovpnProcess.WaitForExit(10 * 1000);
            _ovpnProcess.OutputDataReceived -= OutputDataReceived;
            _ovpnProcess.ErrorDataReceived -= ErrorDataReceived;
            _ovpnProcess.CancelOutputRead();
            _ovpnProcess.CancelErrorRead();
            IsConnected = false;
        }

        /// <summary>
        /// Disposes the OpenVPN manager
        /// </summary>
        public void Dispose()
        {
            _errorLogger.Write("(Manager/Dispose) Disposing myself...");
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects to the OpenVPN management server on port 33333
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Completed task</returns>
        internal async Task ConnectToManagementServerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_managementSocket is null || !_managementSocket.Connected)
                {
                    CreateSocket();

                    _errorLogger.Write($"(Manager/ConnectToManagementServer) Establishing (sock type: {_managementSocket.SocketType}, proto: {_managementSocket.ProtocolType}) connection to endpoint: {_managementEp}");
                    await _managementSocket.ConnectAsync(_managementEp, cancellationToken);
                }
            }
            catch (ObjectDisposedException)
            {
                CreateSocket();
                await ConnectToManagementServerAsync(cancellationToken);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Creates the socket and endpoint, then assigns them to the properties.
        /// </summary>
        internal void CreateSocket()
        {
            _errorLogger.Write("(Manager/CreateSocket) Creating new socket & endpoint");

            var ip = IPAddress.Parse("127.0.0.1");
            _managementEp = new IPEndPoint(ip, 33333);

            _managementSocket = new(ip.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Sends a buffer to the connected OpenVPN management socket.
        /// </summary>
        /// <param name="buffer">
        /// The buffer you want to send, gets converted to bytes and sent through the socket
        /// </param>
        /// <param name="shouldRecv">
        /// Should we attempt to receive back from the socket, if not the return will always be null
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// The received back buffer from the socket, will be null if <paramref name="shouldRecv" />
        /// is false
        /// </returns>
        internal async Task<string> SendBufferToManagementServer(string buffer, bool shouldRecv, CancellationToken cancellationToken = default)
        {
            try
            {
                _errorLogger.Write("(Manager/SendBuffer) Checking socket connection status...");
                await ConnectToManagementServerAsync(cancellationToken);

                if (_managementSocket is null || !_managementSocket.Connected)
                {
                    _errorLogger.Write("(Manager/SendBuffer) Socket is null or not connected after ConnectToManagementServer called, the management server is dead, returning null");
                    return null;
                }

                byte[] bytes = new byte[1024];

                byte[] msg = Encoding.UTF8.GetBytes(buffer + "\r\n"); // \r\n is required because OpenVPN Management Interface says so (and it likes legacy).

                int sendBytes = await _managementSocket.SendAsync(msg, SocketFlags.None, cancellationToken);
                _errorLogger.Write($"(Manager/SendBuffer) Sent {sendBytes} bytes through Socket");

                if (shouldRecv)
                {
                    int recvBytes = await _managementSocket.ReceiveAsync(bytes, SocketFlags.None, cancellationToken);
                    _errorLogger.Write($"(Manager/SendBuffer) Recv {recvBytes} bytes back from Socket");

                    return Encoding.UTF8.GetString(bytes);
                }
                return null;
            }
            catch (Exception e)
            {
                _errorLogger.Write($"(Manager/SendBuffer) Exception when sending buffer:\n{e}");
                return null;
            }
        }

        /// <summary>
        /// Shuts down the OpenVPN management server asynchronously
        /// </summary>
        /// <returns>Completed task</returns>
        internal async Task ShutdownManagementServerAsync()
        {
            if (_managementSocket is not null && _managementSocket.Connected)
            {
                await SendBufferToManagementServer("signal SIGTERM", false);

                _managementSocket.Shutdown(SocketShutdown.Both);
                _managementSocket.Close();
                _errorLogger.Write("(Manager/ShutdownManagementServer) Shut down and closed ManagementSocket");
            }
        }

        /// <summary>
        /// Disposes the manager
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _retryCount = 0;
                if (disposing)
                {
                    _ovpnProcess.Kill();

                    if (_ovpnProcess != null)
                    {
                        _ovpnProcess.WaitForExit();
                    }
                }
                IsDisposed = true;
            }
        }

        private static async Task RefetchConfigsAsync()
        {
            await Globals.container.GetInstance<IHttp>().CacheConfigsAsync(true);
        }

        private static void ReinstallTap()
        {
            var instance = Globals.container.GetInstance<ITapManager>();
            if (instance.IsAdapterExistant())
            {
                instance.RemoveTapAdapter();
            }
            instance.CreateTapAdapter();
        }

        /// <summary>
        /// The error data recieved event, invoked whenever an error is thrown by OpenVPN
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputReceived onOutput = this.OnOutput;
            if (Output == null)
            {
                return;
            }
            Output.Invoke(this, OutputType.Error, e.Data);
        }

        /// <summary>
        /// Invoked when an error occurrs, prevents a error loop
        /// </summary>
        /// <param name="message"></param>
        private void InvokeError(string message)
        {
            Disconnect();

            // this really pisses me off with how gay it is but i dont want to violate dry that much
            if (Error == null) return;
            Error.Invoke(this, message);
        }

        /// <summary>
        /// The regular output data recieved event, invoked whenever OpenVPN outputs something to
        /// the console
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private async void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            await _logger.WriteAsync(e.Data);

            if (e.Data.Contains("Received control message: AUTH_FAILED"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) Recieved control message: AUTH_FAILED");
                await PerformAutoTroubleshootAsync(true, $"Authentication to the VPN server has failed, your plan could've expired. Check https://lightvpn.org/dashboard");
                return;
            }
            else if (e.Data.Contains("Error opening configuration file"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) Failed to open config");
                await PerformAutoTroubleshootAsync(true, "Error opening configuration file, your antivirus could be blocking LightVPN as we couldn't re-fetch them.");
                return;
            }
            else if (e.Data.Contains("Exiting due to fatal error"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) OpenVPN CLI has exited due to fatal error");
                await PerformAutoTroubleshootAsync(false, "OpenVPN has exited unexpectedly, this could be due to a TAP adapter issue.");
                return;
            }
            else if (e.Data.Contains("Server poll timeout"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) Server conn timeout");
                await PerformAutoTroubleshootAsync(true, "Timed out connecting to server, the server could currently be down. Check https://lightvpn.org/locations to see server status.");
                return;
            }
            else if (e.Data.Contains("Unknown error"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) Unknown error (this is not good)");
                await PerformAutoTroubleshootAsync(false, "Unknown error connecting to server, reinstall your TAP adapter and try again");
                return;
            }
            else if (e.Data.Contains("Adapter 'LightVPN-TAP' not found"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) No OVPN-TAP");
                await PerformAutoTroubleshootAsync(false, "Couldn't find TAP adapter, reinstall your TAP adapter and try again");
                return;
            }
            else if (e.Data.Contains("Initialization Sequence Completed"))
            {
                _errorLogger.Write("(Manager/OpenVpnOutputHandler) We connected sir!");
                if (Connected == null) return;
                Connected.Invoke(this);

                await ConnectToManagementServerAsync();
            }
            else
            {
                if (Output == null) return;
                Output.Invoke(this, OutputType.Error, e.Data);
            }
        }

        private async Task PerformAutoTroubleshootAsync(bool isServerRelated, string invokationMessage)
        {
            if (_retryCount >= 1)
            {
                _retryCount = 0;
                InvokeError($"Automated troubleshooting has failed!\n\n{invokationMessage}");
                return;
            }
            if (!isServerRelated)
            {
                _retryCount++;

                Disconnect();

                ReinstallTap();

                Connect(_config);
                return;
            }
            else
            {
                _retryCount++;

                Disconnect();

                await RefetchConfigsAsync();

                Connect(_config);
                return;
            }
        }

        /// <summary>
        /// Starts the OpenVPN process, and connects to the specified config, and the TAP adapter
        /// </summary>
        /// <param name="ovpn">Path to the OpenVPN config file</param>
        private void RunOpenVpnProcess(string ovpn)
        {
            _errorLogger.Write("(Manager/RunOpenVpnProcess) Configuring and booting OpenVPN CLI...");
            _ovpnProcess.StartInfo.CreateNoWindow = true;
            _ovpnProcess.StartInfo.Arguments = $"--config \"{_config}\" --register-dns --dev-node LightVPN-TAP --management 127.0.0.1 33333";
            _ovpnProcess.StartInfo.FileName = _ovpnPath;
            _ovpnProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _ovpnProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(ovpn);
            _ovpnProcess.StartInfo.RedirectStandardInput = true;
            _ovpnProcess.StartInfo.RedirectStandardOutput = true;
            _ovpnProcess.StartInfo.RedirectStandardError = true;
            _ovpnProcess.StartInfo.UseShellExecute = false;
            _ovpnProcess.StartInfo.Verb = "runas";
            _ovpnProcess.OutputDataReceived += OutputDataReceived;
            _ovpnProcess.ErrorDataReceived += ErrorDataReceived;
            _ovpnProcess.Start();
            _errorLogger.Write("(Manager/RunOpenVpnProcess) Booted!");
            _ovpnProcess.BeginOutputReadLine();
            _ovpnProcess.BeginErrorReadLine();
            ChildProcessTracker.AddProcess(_ovpnProcess);
            _errorLogger.Write("(Manager/RunOpenVpnProcess) Redirected stdout && added to ChildProcessTracker");
        }
    }
}