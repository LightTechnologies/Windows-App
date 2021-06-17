using System;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.OpenVPN.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightVPN.Client.OpenVPN
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddOpenVpn(this IServiceCollection services,
            Action<OpenVpnConfiguration> configuration)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var config = new OpenVpnConfiguration();
            configuration(config);

            return services.AddSingleton<IVpnManager>(new VpnManager(config));
        }
    }
}