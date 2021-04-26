/* --------------------------------------------
 * 
 * OpenVPN TAP Manager - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: TapManager.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */

using LightVPN.Auth;
using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.OpenVPN.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN
{
    /// <summary>
    /// The class for managing OpenVPN TAP adapters
    /// </summary>
    public class TapManager : ITapManager
    {
        private string _path;
        private Process _proc;
        /// <summary>
        /// Constructs the TAP manager, with the specified path for the 'tapctl' binary
        /// </summary>
        /// <param name="path">Path to the tapctl binary</param>
        /// <param name="driverpath">Just the path to the drivers</param>
        public TapManager(string path = @"C:\Program Files\OpenVPN\bin\tapctl.exe")
        {
            _path = path;
        }

        public Task InstallDriverAsync()
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Globals.OpenVpnDriversPath,
                    CreateNoWindow = true,
                    Arguments = "install OemVista.inf tap0901",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Path.Combine(Globals.OpenVpnDriversPath, "tapinstall.exe")
                }
            };
            _proc.Start();
            _proc.WaitForExit();

            return Task.CompletedTask;
        }

        public Task RemoveDriversAsync()
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Globals.OpenVpnDriversPath,
                    CreateNoWindow = true,
                    Arguments = "remove tap0901",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Path.Combine(Globals.OpenVpnDriversPath, "tapinstall.exe")
                }
            };
            _proc.Start();
            _proc.WaitForExit();
            return Task.CompletedTask;
        }

        public bool CheckDriverExists()
        {
            bool found = false;
            var query = new SelectQuery("select * from Win32_PnPSignedDriver");
#pragma warning disable CA1416 // Validate platform compatibility
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            foreach (ManagementObject service in searcher.Get())
#pragma warning restore CA1416 // Validate platform compatibility
            {
                if (found) break;
                var name = service["Description"]?.ToString();
                found = name == "TAP-Windows Adapter V9";
            }
            return found;
        }
        /// <summary>
        /// Creates a TAP adapter with the specified friendly name, defaults to LightVPN-TAP
        /// </summary>
        /// <param name="name">Friendly name of the adapter</param>
        public void CreateTapAdapter(string name = "LightVPN-TAP")
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = $"create --name {name}",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = _path
                }
            };
            _proc.Start();
            _proc.WaitForExit();
        }
        /// <summary>
        /// Removes a TAP adapter with the specified friendly name, defaults to LightVPN-TAP
        /// </summary>
        /// <param name="name">Friendly name of the adapter</param>
        public void RemoveTapAdapter(string name = "LightVPN-TAP")
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = $"delete {name}",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = _path
                }
            };
            _proc.Start();
            _proc.WaitForExit();
        }

        /// <summary>
        /// Checks if the specified TAP adapter exists on the system
        /// </summary>
        /// <param name="name">Friendly name of the adapter</param>
        /// <returns>True or false value of whether the LightVPN TAP adapter was found</returns>
        public bool IsAdapterExistant(string name = "LightVPN-TAP")
        {
            bool found = false;
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = "list",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = _path
                }
            };
            _proc.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null && e.Data.Contains(name) && !found)
                {
                    found = true;
                }
            };
            _proc.Start();
            _proc.BeginOutputReadLine();
            _proc.WaitForExit();
            return found;
        }
        /// <summary>
        /// Disposes the TapManager class
        /// </summary>
        public void Dispose()
        {
            _proc.Dispose();
            _proc = null;
            _path = null;
            GC.SuppressFinalize(this);
        }
    }
}
