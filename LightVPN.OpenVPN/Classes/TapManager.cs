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

using LightVPN.Common.Models;
using LightVPN.OpenVPN.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN
{
    /// <summary>
    /// The class for managing OpenVPN TAP adapters on Windows clients
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class TapManager : ITapManager
    {
        private string _path;

        private Process _proc;

        /// <summary>
        /// Constructs the TAP manager, with the specified path for the 'tapctl' binary
        /// </summary>
        /// <param name="path">Path to the tapctl binary</param>
        public TapManager(string path = @"C:\Program Files\OpenVPN\bin\tapctl.exe")
        {
            _path = path;
        }

        /// <summary>
        /// Checks if the OpenVPN TAP driver has been installed
        /// </summary>
        /// <returns>True if it is installed, false if it isn't</returns>
        public bool CheckDriverExists()
        {
            bool found = false;
            var query = new SelectQuery("select * from Win32_PnPSignedDriver");
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject service in searcher.Get())
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
        /// Disposes the TapManager class
        /// </summary>
        public void Dispose()
        {
            _proc.Dispose();
            _proc = null;
            _path = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Installs the OpenVPN TAP drivers asynchronously
        /// </summary>
        /// <returns>Completed task</returns>
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
        /// Removes the OpenVPN TAP driver asynchronously
        /// </summary>
        /// <returns>Completed task</returns>
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
    }
}