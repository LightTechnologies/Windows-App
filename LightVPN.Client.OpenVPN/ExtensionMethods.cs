using System;
using LightVPN.Client.OpenVPN.Interfaces;
using LightVPN.Client.OpenVPN.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightVPN.Client.OpenVPN
{
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Adds a instance of IVpnManager to the service collection
        /// </summary>
        /// <param name="services">The existing service collection</param>
        /// <param name="configuration">The OpenVPN configuration</param>
        /// <returns>The same service collection but with IVpnManager injected into it</returns>
        /// <exception cref="ArgumentNullException"></exception>
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