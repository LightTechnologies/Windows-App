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
using LightVPN.OpenVPN.Interfaces;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using LightVPN.Logger;
using LightVPN.Common.v2.Models;
using LightVPN.Logger.Base;

namespace LightVPN.OpenVPN
{
    public class Manager : IManager
    {
        /// <summary>
        /// Starts the OpenVPN process, and connects to the specified config, and the TAP adapter
        /// </summary>
        /// <param name="ovpn">Path to the OpenVPN config file</param>
        private void RunOpenVpnProcess(string ovpn)
        {
            this.prc.StartInfo.CreateNoWindow = true;
            this.prc.StartInfo.Arguments = $"--config \"{this.config}\" --dev-node {this.tap}";
            this.prc.StartInfo.FileName = this.openVpnExePath;
            this.prc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.prc.StartInfo.WorkingDirectory = Path.GetDirectoryName(ovpn);
            this.prc.StartInfo.RedirectStandardOutput = true;
            this.prc.StartInfo.RedirectStandardError = true;
            this.prc.StartInfo.UseShellExecute = false;
            this.prc.OutputDataReceived += this.Prc_OutputDataReceived;
            this.prc.ErrorDataReceived += this.Prc_ErrorDataReceived;
            this.prc.Start();
            this.prc.BeginOutputReadLine();
            this.prc.BeginErrorReadLine();
            ChildProcessTracker.AddProcess(this.prc);
        }

        public event Manager.outputRecieved OnOutput;
        public event Manager.OutputEvent Output;
        public event Manager.LoginFailedEvent LoginFailed;
        public event Manager.ConnectedEvent Connected;
        public event Manager.ErrorEvent Error;
        /// <summary>
        /// The error data recieved event, invoked whenever an error is thrown by OpenVPN
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Manager.outputRecieved onOutput = this.OnOutput;
            if (Output == null)
            {
                return;
            }
            Output.Invoke(this, OutputType.Error, e.Data);
        }
        /// <summary>
        /// The regular output data recieved event, invoked whenever OpenVPN outputs something to the console
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private async void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            await logger.WriteAsync(e.Data);

            if (e.Data.Contains("Received control message: AUTH_FAILED"))
            {
                if (LoginFailed == null) return;
                LoginFailed.Invoke(this);
                Disconnect();
            }
            else if (e.Data.Contains("Error opening configuration file"))
            {
                InvokeError("Error opening configuration file, you should clear your VPN server cache");
            }
            else if (e.Data.Contains("Server poll timeout"))
            {
                InvokeError("Timed out connecting to server"); // yes i know i can use Output?.Invoke i just dont want to as its not as clean
            }
            else if(e.Data.Contains("Unknown error"))
            {
                InvokeError("Unknown error connecting to server, reinstall your TAP adapter and try again");
            }
            else if (e.Data.Contains("Initialization Sequence Completed"))
            {
                if (Connected == null) return;
                Connected.Invoke(this);
            }
            else
            {
                if (Output == null) return;
                Output.Invoke(this, OutputType.Error, e.Data);
            }
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
        /// Constructs the OpenVPN manager class
        /// </summary>
        /// <param name="openVpnExeFileName">Path to the OpenVPN binary</param>
        /// <param name="tap">Friendly name of the OpenVPN TAP adapter</param>
        public Manager(string openVpnExeFileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe", string tap = "LightVPN-TAP")
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
            this.openVpnExePath = openVpnExeFileName;
            this.tap = tap;
            this.IsDisposed = false;
        }
        /// <summary>
        /// Connects to the specified OpenVPN config file
        /// </summary>
        /// <param name="configpath">Path to the configuration file</param>
        public void Connect(string configpath)
        {
            if (IsConnected) throw new Exception("Already connected to the VPN");
            this.config = configpath;
            this.RunOpenVpnProcess(openVpnExePath);
            IsConnected = true;
        }
        /// <summary>
        /// Disconnects from any VPN currently connected
        /// </summary>
        public void Disconnect()
        {
            prc.OutputDataReceived -= Prc_OutputDataReceived;
            prc.ErrorDataReceived -= Prc_ErrorDataReceived;
            prc.CancelOutputRead();
            prc.CancelErrorRead();
            prc.Kill();
            prc.WaitForExit(1000);
            IsConnected = false;
        }
        public bool IsConnected { get; private set; }

        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Disposes the OpenVPN manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.prc.Kill();

                    if (this.prc != null)
                    {
                        this.prc.WaitForExit();
                    }
                }
                this.IsDisposed = true;
            }
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

        private readonly FileLogger logger = new OpenVpnLogger(Globals.OpenVpnLogPath);

        private string config;

        private readonly string tap;

        private readonly Process prc = new();

        private readonly string openVpnExePath;
        /// <summary>
        /// Defines what types of output can be thrown
        /// </summary>
        public enum OutputType
        {
            Normal,
            Connected,
            Error
        }
        public delegate void outputRecieved(object sender, DataReceivedEventArgs e);
        public delegate void OutputEvent(object sender, OutputType e, string message);
        public delegate void LoginFailedEvent(object sender);
        public delegate void ConnectedEvent(object sender);
        public delegate void ErrorEvent(object sender, string message);
    }
}
