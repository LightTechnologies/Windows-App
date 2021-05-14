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
using System.Threading.Tasks;

namespace LightVPN.OpenVPN.Utils.Linux
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
        /// <param name="path">Path to the OpenVPN binary</param>
        public TapManager(string path = @"/usr/bin/openvpn")
        {
            _path = path;
        }

        /// <summary>
        /// Checks if the OpenVPN TAP driver has been installed
        /// </summary>
        /// <returns>True if it is installed, false if it isn't</returns>
        public bool CheckDriverExists()
        {
            if (!File.Exists("/usr/bin/openvpn"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a TAP adapter with the specified interface name, defaults to 'tun0'
        /// </summary>
        /// <param name="name">Interface name of the adapter</param>
        public void CreateTapAdapter(string name = "tun0")
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = $"--mktun --dev {name}",
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
            throw new NotSupportedException("This operation is not supported on Linux");
        }

        /// <summary>
        /// Checks if the specified TAP adapter exists on the system
        /// </summary>
        /// <param name="name">Friendly name of the adapter</param>
        /// <returns>True or false value of whether the LightVPN TAP adapter was found</returns>
        public bool IsAdapterExistant(string name = "tun0")
        {
            // This is a hacky solution but it will work
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = $"a",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/usr/bin/ip"
                }
            };
            _proc.Start();
            _proc.BeginOutputReadLine();

            bool found = false;

            _proc.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data)) return;

                if (e.Data.Contains(name)) found = true;
            };
            _proc.WaitForExit();

            return found;
        }

        /// <summary>
        /// Removes the OpenVPN TAP driver asynchronously
        /// </summary>
        /// <returns>Completed task</returns>
        public Task RemoveDriversAsync()
        {
            throw new NotSupportedException("This operation is not supported on Linux");
        }

        /// <summary>
        /// Removes a TAP adapter with the specified interface name, defaults to 'tun0'
        /// </summary>
        /// <param name="name">Interface name</param>
        public void RemoveTapAdapter(string name = "tun0")
        {
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_path),
                    CreateNoWindow = true,
                    Arguments = $"--rmtun --dev {name}",
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