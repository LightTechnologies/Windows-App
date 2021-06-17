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
    internal static class Startup
    {
        internal static void Run()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IApiClient>();
        }

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