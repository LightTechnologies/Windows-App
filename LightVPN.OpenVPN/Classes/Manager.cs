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
using LightVPN.OpenVPN.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
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

        private ushort _managementPort;

        private Socket _managementSocket;

        private ushort _retryCount;

        private PlatformID _platform;

        /// <summary>
        /// Constructs the OpenVPN manager class
        /// </summary>
        /// <param name="openVpnExeFileName">Path to the OpenVPN binary (if the platform is Linux make sure it's /usr/bin/openvpn or whereever the binary is located at)</param>
        public Manager(string openVpnExeFileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe", PlatformID platform = PlatformID.Win32NT)
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
            _platform = platform;
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

        public ushort ManagementPort
        {
            get
            {
                return _managementPort;
            }

            set

            {
                _managementEp = SocketUtils.GetEndPoint(value);
                _errorLogger.Write($"(Manager/ManagementPort;set) Got new endpoint; {_managementEp}");
                _managementPort = value;
            }
        }

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
        /// Gets the connection state via the management socket (this should only be used on Linux)
        /// </summary>
        /// <returns>True if the connection is successful (OpenVPN is connected) false otherwise.</returns>
        public async Task<bool> GetConnectionState()
        {
            await ConnectToManagementServerAsync();
            return _managementSocket.Connected;
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

        public async Task PerformAutoTroubleshootAsync(bool isServerRelated, string invokationMessage, CancellationToken cancellationToken = default)
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

                await RefetchConfigsAsync(cancellationToken);

                Connect(_config);
                return;
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

        private static async Task RefetchConfigsAsync(CancellationToken cancellationToken = default)
        {
            await Globals.Container.GetInstance<IHttp>().CacheConfigsAsync(true, cancellationToken);
        }

        private static void ReinstallTap()
        {
            var instance = Globals.Container.GetInstance<ITapManager>();
            if (instance.IsAdapterExistant())
            {
                instance.RemoveTapAdapter();
            }
            instance.CreateTapAdapter();
        }

        /// <summary>
        /// Connects to the OpenVPN management server on a random, available port
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Completed task</returns>
        private async Task ConnectToManagementServerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_managementSocket is null || !_managementSocket.Connected)
                {
                    CreateSocket();

                    _errorLogger.Write($"(Manager/ConnectToManagementServer) Establishing (sock type: {_managementSocket.SocketType}, proto: {_managementSocket.ProtocolType}) connection to endpoint: {_managementEp}");

                    await _managementSocket.ConnectAsync(_managementEp, cancellationToken);

                    _errorLogger.Write($"(Manager/ConnectToManagementServer) Established! ({_managementSocket.Connected})");
                }
            }
            catch (ObjectDisposedException)
            {
                CreateSocket();
                await ConnectToManagementServerAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _errorLogger.Write($"(Manager/ConnectToManagementServer) Ignored exception:\n{e}");
                return;
            }
        }

        /// <summary>
        /// Creates the socket and endpoint, then assigns them to the properties.
        /// </summary>
        private void CreateSocket()
        {
            _errorLogger.Write("(Manager/CreateSocket) Creating new socket & endpoint");

            if (ManagementPort == 0) ManagementPort = SocketUtils.GetAvailablePort(30000);

            _managementSocket = SocketUtils.GetSocket(_managementEp);
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

            switch (e.Data)
            {
                case string str when str.Contains("Received control message: AUTH_FAILED"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) Recieved control message: AUTH_FAILED");
                    await PerformAutoTroubleshootAsync(true, $"Authentication to the VPN server has failed, your plan could've expired. Check https://lightvpn.org/dashboard");

                    return;

                case string str when str.Contains("MANAGEMENT: Socket bind failed on local address"):
                    InvokeError($"Failed to bind socket on local address. To fix this issue, restart LightVPN or try again.");

                    return;

                case string str when str.Contains("Error opening configuration file"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) Failed to open config");
                    await PerformAutoTroubleshootAsync(true, "Error opening configuration file, your antivirus could be blocking LightVPN as we couldn't re-fetch them.");

                    return;

                case string str when str.Contains("Exiting due to fatal error"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) OpenVPN CLI has exited due to fatal error");
                    await PerformAutoTroubleshootAsync(false, "OpenVPN has exited unexpectedly, this could be due to a TAP adapter issue.");

                    return;

                case string str when str.Contains("Server poll timeout"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) Server conn timeout");
                    await PerformAutoTroubleshootAsync(true, "Timed out connecting to server, the server could currently be down. Check https://lightvpn.org/locations to see server status.");

                    return;

                case string str when str.Contains("Unknown error"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) Unknown error (this is not good)");
                    await PerformAutoTroubleshootAsync(false, "Unknown error connecting to server, reinstall your TAP adapter and try again");

                    return;

                case string str when str.Contains("Adapter 'LightVPN-TAP' not found"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) No OVPN-TAP");
                    await PerformAutoTroubleshootAsync(false, "Couldn't find TAP adapter, reinstall your TAP adapter and try again");

                    return;

                case string str when str.Contains("Initialization Sequence Completed"):
                    _errorLogger.Write("(Manager/OpenVpnOutputHandler) We connected sir!");
                    if (Connected == null) return;
                    Connected.Invoke(this);

                    await ConnectToManagementServerAsync();

                    break;

                default:
                    if (Output == null) return;
                    Output.Invoke(this, OutputType.Error, e.Data);

                    break;
            }
        }

        /// <summary>
        /// Starts the OpenVPN process, and connects to the specified config, and the TAP adapter
        /// </summary>
        /// <param name="ovpn">Path to the OpenVPN config file</param>
        private void RunOpenVpnProcess(string ovpn)
        {
            ManagementPort = SocketUtils.GetAvailablePort(30000);

            _ovpnProcess.StartInfo = new()
            {
                CreateNoWindow = true,
                Arguments = _platform == PlatformID.Win32NT ? $"--config \"{_config}\" --register-dns --dev-node LightVPN-TAP --management 127.0.0.1 {ManagementPort}" : $"--config \"{_config}\" --register-dns --management 127.0.0.1 {ManagementPort}",
                FileName = _ovpnPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(ovpn),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Verb = "runas",
            };

            _ovpnProcess.OutputDataReceived += OutputDataReceived;
            _ovpnProcess.ErrorDataReceived += ErrorDataReceived;
            _ovpnProcess.Start();
            _ovpnProcess.BeginOutputReadLine();
            _ovpnProcess.BeginErrorReadLine();
            if (_platform == PlatformID.Win32NT)
            {
                ChildProcessTracker.AddProcess(_ovpnProcess);
            }
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
        private async Task<string> SendBufferToManagementServer(string buffer, bool shouldRecv, CancellationToken cancellationToken = default)
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

                _managementSocket.SendBuffer(Encoding.UTF8.GetBytes(buffer + "\r\n"));

                _errorLogger.Write($"(Manager/SendBuffer) Sent {buffer.Length} bytes through Socket");

                if (shouldRecv)
                {
                    var buffer1 = _managementSocket.ReceiveBuffer();

                    _errorLogger.Write($"(Manager/SendBuffer) Recv {buffer1.Length} bytes back from Socket");

                    return buffer1;
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
        private async Task ShutdownManagementServerAsync(CancellationToken cancellationToken = default)
        {
            if (_managementSocket is not null && _managementSocket.Connected)
            {
                await SendBufferToManagementServer("signal SIGTERM", false, cancellationToken);

                _managementSocket.Shutdown(SocketShutdown.Both);
                _managementSocket.Close();
                _errorLogger.Write("(Manager/ShutdownManagementServer) Shut down and closed ManagementSocket");
            }
        }
    }
}