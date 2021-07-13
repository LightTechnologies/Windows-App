namespace LightVPN.Client.OpenVPN.Utils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Common;
    using Debug;
    using Models;
    using Resources;
    using ErrorEventArgs = EventArgs.ErrorEventArgs;

    /// <summary>
    ///     Manages the TAP adapter and it's drivers, this class is only functional on Windows
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class TapManager
    {
        private Process _tapCtlProcess;
        private readonly OpenVpnConfiguration _configuration;

        public delegate void ErrorReceived(object sender, ErrorEventArgs e);

        public delegate void Success(object sender);

        public event Success OnSuccess;
        public event ErrorReceived OnErrorReceived;

        internal TapManager(OpenVpnConfiguration configuration)
        {
            this._configuration = configuration;

            this.ConstructProcess();
        }

        /// <summary>
        ///     Starts the TAP process and starts redirecting stdout
        /// </summary>
        private void StartProcess()
        {
            this._tapCtlProcess.Start();

            this._tapCtlProcess.BeginErrorReadLine();
            this._tapCtlProcess.BeginOutputReadLine();
        }

        /// <summary>
        ///     Configures the process for TAP driver installation
        /// </summary>
        public void ConfigureTapDriverInstallation()
        {
            this._tapCtlProcess.StartInfo.WorkingDirectory = Globals.OpenVpnDriversPath;
            this._tapCtlProcess.StartInfo.FileName = Path.Combine(Globals.OpenVpnDriversPath, "tapinstall.exe");
        }

        /// <summary>
        ///     Constructs the default process object
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the OpenVpnPath is null</exception>
        private void ConstructProcess()
        {
            this._tapCtlProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    FileName = this._configuration.TapCtlPath,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Path.GetDirectoryName(this._configuration.OpenVpnPath) ??
                                       throw new ArgumentNullException(nameof(TapManager._configuration)),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            this._tapCtlProcess.OutputDataReceived += this.TapCtlProcessOnOutputDataReceived;
            this._tapCtlProcess.ErrorDataReceived += this.TapCtlProcessOnErrorDataReceived;
        }

        /// <summary>
        ///     Installs the TAP adapter driver asynchronously
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task InstallTapDriverAsync(CancellationToken cancellationToken = default)
        {
            this.ConfigureTapDriverInstallation();

            this._tapCtlProcess.StartInfo.Arguments = "install OemVista.inf tap0901";

            this.StartProcess();
            await this.BeginTerminatingProcessAsync(cancellationToken);
        }

        /// <summary>
        ///     Removes the TAP adapter driver asynchronously
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task RemoveTapDriverAsync(CancellationToken cancellationToken = default)
        {
            this.ConfigureTapDriverInstallation();

            this._tapCtlProcess.StartInfo.Arguments = "remove tap0901";

            this.StartProcess();
            await this.BeginTerminatingProcessAsync(cancellationToken);
        }

        /// <summary>
        ///     Checks if the TAP driver has been installed on the system
        /// </summary>
        /// <returns>True if it has been, false otherwise</returns>
        public static bool IsDriverInstalled()
        {
            var found = false;

            var query = new SelectQuery("SELECT * FROM Win32_PnPSignedDriver");

            using var searcher = new ManagementObjectSearcher(query);

            foreach (var item in searcher.Get())
            {
                if (found) break;

                var name = item["Description"]?.ToString();
                found = name == "TAP-Windows Adapter V9";
            }

            return found;
        }

        /// <summary>
        ///     Begins to wait for the process to exit and then cancels stdout redirection
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        private async Task BeginTerminatingProcessAsync(CancellationToken cancellationToken = default)
        {
            await this._tapCtlProcess.WaitForExitAsync(cancellationToken);

            this._tapCtlProcess.CancelOutputRead();
            this._tapCtlProcess.CancelErrorRead();
        }

        /// <summary>
        ///     Installs a TAP adapter on the system with the specified TAP adapter name
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task InstallTapAdapterAsync(CancellationToken cancellationToken = default)
        {
            this._tapCtlProcess.StartInfo.Arguments = $"create --name {this._configuration.TapAdapterName}";

            this.StartProcess();
            await this.BeginTerminatingProcessAsync(cancellationToken);
        }

        /// <summary>
        ///     Removes a TAP adapter from the system with the specified TAP adapter name
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task RemoveTapAdapterAsync(CancellationToken cancellationToken = default)
        {
            this._tapCtlProcess.StartInfo.Arguments = $"delete {this._configuration.TapAdapterName}";

            this.StartProcess();
            await this.BeginTerminatingProcessAsync(cancellationToken);
        }

        /// <summary>
        ///     Checks if the TAP adapter specified is existent on the system
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if it is, false otherwise</returns>
        public async Task<bool> IsAdapterExistentAsync(CancellationToken cancellationToken = default)
        {
            var found = false;

            this._tapCtlProcess.StartInfo.Arguments = "list";

            this._tapCtlProcess.OutputDataReceived -= this.TapCtlProcessOnOutputDataReceived;

            this._tapCtlProcess.OutputDataReceived += (_, args) =>
            {
                if (args.Data?.Contains(this._configuration.TapAdapterName) == true && !found) found = true;
            };

            this._tapCtlProcess.OutputDataReceived += this.TapCtlProcessOnOutputDataReceived;

            this.StartProcess();

            await this.BeginTerminatingProcessAsync(cancellationToken);

            return found;
        }

        /// <summary>
        ///     Fired when error data is received from the tapctl process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException">Thrown when TAP creation has failed</exception>
        private void TapCtlProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            DebugLogger.Write("lvpn-client-ovpn-tapman", $"create_vpn_adap failed: {e.Data}");

            this._tapCtlProcess.Kill();

            this.OnErrorReceived?.Invoke(this,
                new ErrorEventArgs(
                    new InvalidOperationException(
                        "Failed to create VPN adapter. Your OpenVPN drivers are most likely corrupt.")));
        }

        /// <summary>
        ///     Fired when normal output data is received from the tapctl process, any errors are handled in this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong</exception>
        private void TapCtlProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            switch (e.Data)
            {
                case var str
                    when str.Contains(string.Format(StringTable.OVPN_TAP_ALREADY_EXISTS,
                        this._configuration.TapAdapterName)):
                    this.OnErrorReceived?.Invoke(this,
                        new ErrorEventArgs(
                            new InvalidOperationException("TAP adapter already exists!")));
                    break;
                case var str
                    when str.Contains(string.Format(StringTable.OVPN_TAP_NO_EXISTS,
                        this._configuration.TapAdapterName)):
                    this.OnErrorReceived?.Invoke(this,
                        new ErrorEventArgs(
                            new InvalidOperationException("TAP adapter doesn't exist!")));
                    break;
                case var guid when Guid.TryParse(guid, out var _):
                    DebugLogger.Write("lvpn-client-ovpn-tapman", "tapctl exited, tap created it seems.");
                    this.OnSuccess?.Invoke(this);
                    break;
            }
        }
    }
}
