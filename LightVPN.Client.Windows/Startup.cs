using System.IO;
using LightVPN.Client.Auth;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.OpenVPN;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Services;
using LightVPN.Client.Windows.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LightVPN.Client.Windows
{
    /// <summary>
    /// Handles the injection of services
    /// </summary>
    internal static class Startup
    {
        /// <summary>
        /// Creates a new service collection and configures all the services (this does not startup the WPF UI)
        /// </summary>
        internal static void Run()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IApiClient>();
        }

        /// <summary>
        /// Injects and configures all the services
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApiClient, ApiClient>()
                .AddOpenVpn(config =>
                {
                    config.OpenVpnPath = Path.Combine(Globals.OpenVpnPath, "openvpn.exe");
                    config.TapAdapterName = "LightVPN-TAP";
                    config.TapCtlPath = Path.Combine(Globals.OpenVpnPath, "tapctl.exe");
                    config.OpenVpnLogPath = Path.Combine(Globals.OpenVpnLogPath);
                })
                .AddSingleton<IOpenVpnService, OpenVpnService>();
        }
    }
}