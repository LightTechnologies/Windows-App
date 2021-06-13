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
using LightVPN.OpenVPN.Utils.Linux;
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
    /// <summary>
    /// The main OpenVPN manager class, handles connecting, disconnecting and the OpenVPN process as a whole.
    /// </summary>
    public class Manager : IManager
    {
        private readonly FileLoggerBase _errorLogger = new ErrorLogger();

        private readonly FileLoggerBase _logger = new OpenVpnLogger();

        private readonly string _ovpnPath;

        private Process _ovpnProcess = new();

        private string _config;

        private EndPoint _managementEp;

        private ushort _managementPort;

        private Socket _managementSocket;

        private ushort _retryCount;

        private PlatformID _platform;

        /// <summary>
        /// Constructs the OpenVPN manager class
        /// </summary>
        /// <param name="openVpnExeFileName">Path to the OpenVPN binary (if the platform is Linux make sure it's /usr/bin/openvpn or where-ever the binary is located at)</param>
        /// <param name="platform">The platform the manager class is working on, depending on this parameter the class will adapt to support the platform</param>
        public Manager(string openVpnExeFileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe", PlatformID platform = PlatformID.Win32NT)
        {
            try
            {
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
        /// <summary>
        /// Gets the connection state from OpenVPN
        /// </summary>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// Whether the class has been disposed or not
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// The port the management server is running on, this can vary if the port needed is being used by another process
        /// </summary>
        public ushort ManagementPort
        {
            get
            {
                return _managementPort;
            }

            set

            {
                _managementEp = SocketUtils.GetEndPoint(value);
                _managementPort = value;
            }
        }

        /// <summary>
        /// Connects to the specified OpenVPN config file
        /// </summary>
        /// <param name="configpath">Path to the configuration file</param>
        public async Task ConnectAsync(string configpath)
        {
            if (IsConnected) throw new Exception("Already connected to the VPN");
            _config = configpath;
            await RunOpenVpnProcessAsync(_ovpnPath);
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
        /// Stops the system from redirecting OpenVPN's output, this is only really used on the Linux client
        /// </summary>
        public void StopStdOutRedirection()
        {
            _ovpnProcess.OutputDataReceived -= OutputDataReceived;
            _ovpnProcess.ErrorDataReceived -= ErrorDataReceived;
            _ovpnProcess.CancelErrorRead();
            _ovpnProcess.CancelOutputRead();
        }

        /// <summary>
        /// Disconnects from any VPN currently connected
        /// </summary>
        public async Task DisconnectAsync()
        {
            await ShutdownManagementServerAsync();
            StopStdOutRedirection();

            await _ovpnProcess.WaitForExitAsync(); // this might lock up the thread it is executed on. From testing it it doesnt seem to lock up. Its non async counterpart does lock the thread without
            // a time being passed into it though
            IsConnected = false;
            _ovpnProcess.Dispose();
            _ovpnProcess = new();
        }

        /// <summary>
        /// Disposes the OpenVPN manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Performs the default troubleshooting steps that we'd normally tell people to do themselves
        /// </summary>
        /// <param name="isServerRelated">Whether the issue is related to the VPN server</param>
        /// <param name="invokationMessage">The message that will pop-up when this method fails to fix the issue</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PerformAutoTroubleshootAsync(bool isServerRelated, string invokationMessage, CancellationToken cancellationToken = default)
        {
            if (_retryCount >= 1)
            {
                _retryCount = 0;
                await InvokeError($"Automated troubleshooting has failed!\n\n{invokationMessage}");
                return;
            }
            try
            {
                if (!isServerRelated)
                {
                    _retryCount++;

                    await DisconnectAsync();

                    ReinstallTap();

                    await ConnectAsync(_config);
                    return;
                }
                else
                {
                    _retryCount++;

                    await DisconnectAsync();

                    await RefetchConfigsAsync(cancellationToken);

                    await ConnectAsync(_config);
                    return;
                }
            }
            catch (Exception e)
            {
                _errorLogger.Write($"(Manager/TroubleShooter) Exception:\n{e}"); 
                await InvokeError($"Exception was thrown during automated troubleshooting.!\n\nPlease report it to support");
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
        /// <summary>
        /// Refetches the configurations files, this is here to clean the code up a little bit
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task RefetchConfigsAsync(CancellationToken cancellationToken = default) => await Globals.Container.GetInstance<IHttp>().CacheConfigsAsync(true, cancellationToken);
        /// <summary>
        /// Reinstalls the TAP adapter, this is also here to clean up the code a little bit
        /// </summary>
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

                    await _managementSocket.ConnectAsync(_managementEp, cancellationToken);
                }
            }
            catch (ObjectDisposedException)
            {
                CreateSocket();
                await ConnectToManagementServerAsync(cancellationToken);
            }
            catch (SocketException e)
            {
                _errorLogger.Write($"Fatal Exception: {e}");
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
            if (Output is null) return;
            Output.Invoke(this, OutputType.Error, e.Data);
        }

        /// <summary>
        /// Invoked when an error occurrs, prevents a error loop
        /// </summary>
        /// <param name="message"></param>
        private async Task InvokeError(string message)
        {
            _logger.Write(message);
            if (_managementSocket.Connected)
                 await DisconnectAsync();
            else
                _logger.Write("We were not connected this could be bad");

            if (Error is null) return;
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


            
            _logger.Write(e.Data);

            switch (e.Data)
            {
                case string str when str.Contains("Using --management on a TCP port WITHOUT passwords is STRONGLY discouraged and considered insecure"):
                    await ConnectToManagementServerAsync();
                    break;
                case string str when str.Contains("Received control message: AUTH_FAILED"):
                    await PerformAutoTroubleshootAsync(true, $"Authentication to the VPN server has failed, your plan could've expired. Check https://lightvpn.org/dashboard");

                    break;

                case string str when str.Contains("MANAGEMENT: Socket bind failed on local address"):
                    await InvokeError($"Failed to bind socket on local address. To fix this issue, restart LightVPN or try again.");

                    break;

                case string str when str.Contains("Error opening configuration file"):
                    await PerformAutoTroubleshootAsync(true, "Error opening configuration file, your antivirus could be blocking LightVPN as we couldn't re-fetch them.");

                    break;

                case string str when str.Contains("Exiting due to fatal error"):
                    await PerformAutoTroubleshootAsync(false, "OpenVPN has exited unexpectedly, this could be due to a TAP adapter issue.");

                    break;

                case string str when str.Contains("Server poll timeout"):
                    await InvokeError("Timed out connecting to server, the server could currently be down. Check https://lightvpn.org/locations to see server status.");

                    break;

                case string str when str.Contains("Unknown error"):
                    await PerformAutoTroubleshootAsync(false, "Unknown error connecting to server, reinstall your TAP adapter and try again");

                    break;

                case string str when str.Contains("Adapter 'LightVPN-TAP' not found"):
                    await PerformAutoTroubleshootAsync(false, "Couldn't find TAP adapter, reinstall your TAP adapter and try again");

                    break;

                case string str when str.Contains("Initialization Sequence Completed"):
                    if (Connected is null) break;

                    Connected.Invoke(this);

                    break;

                default:
                    if (Output is null) break;

                    Output.Invoke(this, OutputType.Error, e.Data);

                    break;
            }
        }

        /// <summary>
        /// Starts the OpenVPN process, and connects to the specified config, and the TAP adapter
        /// </summary>
        /// <param name="ovpn">Path to the OpenVPN config file</param>
        private async Task RunOpenVpnProcessAsync(string ovpn)
        {
            ManagementPort = SocketUtils.GetAvailablePort(30000);

            // Patch OpenVPN on Linux, we use OperatingSystem instead of pLatform because of debugging.
            if (OperatingSystem.IsLinux() && !DnsLeakPatcher.IsDnsLeaksPatched())
            {
                await DnsLeakPatcher.PatchDnsLeaksAsync();
            }

            _ovpnProcess.EnableRaisingEvents = true;

            _ovpnProcess.StartInfo = new()
            {
                CreateNoWindow = true,
                Arguments = _platform == PlatformID.Win32NT ? $"--config \"{_config}\" {(_platform == PlatformID.Unix ? "" : "--register-dns --dev-node LightVPN-TAP")} --management 127.0.0.1 {ManagementPort}" : $"--config \"{_config}\" --management 127.0.0.1 {ManagementPort}",
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
                await ConnectToManagementServerAsync(cancellationToken);

                if (_managementSocket is null || !_managementSocket.Connected)
                {
                    return null;
                }

                _managementSocket.SendBuffer(Encoding.UTF8.GetBytes(buffer + "\r\n"));

                if (shouldRecv)
                {
                    var buffer1 = _managementSocket.ReceiveBuffer();

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
        /// <returns></returns>
        private async Task ShutdownManagementServerAsync(CancellationToken cancellationToken = default)
        {
            if (_managementSocket is not null && _managementSocket.Connected)
            {
                await SendBufferToManagementServer("signal SIGTERM", false, cancellationToken);

                _managementSocket.Shutdown(SocketShutdown.Both);
                _managementSocket.Close();
            }
        }
    }
}