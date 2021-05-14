using LightVPN.OpenVPN.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using WindowsFirewallHelper;
using WindowsFirewallHelper.FirewallRules;

namespace LightVPN.OpenVPN
{
    public class Killswitch : IKillswitch
    {
        private readonly string _interfaceFriendlyName;

        /// <summary>
        /// Makes a new instance of the Killswitch
        /// </summary>
        /// <param name="interfaceFriendlyName">The friendly name of the interface to monitor</param>
        public Killswitch(string interfaceFriendlyName)
        {
            _interfaceFriendlyName = interfaceFriendlyName;
        }

        /// <summary>
        /// Starts killswitch
        /// </summary>
        public void StartKillswitch()
        {
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            Execute(true);
        }

        /// <summary>
        /// stops killswitch
        /// </summary>
        public void StopKillswitch()
        {
            NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
            Execute(false);
        }

        /// <summary>
        /// executes the command to enable the rule and disable the rule
        /// </summary>
        /// <param name="enable"></param>
        private static void Execute(bool enable)
        {
            var rule = FirewallWAS.Instance.Rules.FirstOrDefault(x => x.Description == "LightVPN Killswitch");
            var appRule = FirewallWAS.Instance.Rules.FirstOrDefault(x => x.Description == "LightVPN Application");
            if (rule != null) { FirewallWAS.Instance.Rules.Remove(rule); rule = null; }
            if (appRule != null) { FirewallWAS.Instance.Rules.Remove(appRule); appRule = null; }
            if (FirewallWAS.IsSupported && FirewallWASRuleWin8.IsSupported && enable)
            {
                rule ??= new FirewallWASRuleWin8(
                "Killswitch",
                FirewallAction.Block,
                FirewallDirection.Outbound,
                FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public
            )
                {
                    Description = "LightVPN Killswitch",
                    NetworkInterfaceTypes = NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan,
                    Protocol = FirewallProtocol.Any,
                    Name = "Killswitch"
                };
                var appath = Process.GetCurrentProcess().MainModule.FileName;
                appRule ??= new FirewallWASRuleWin8(
                "LightVPN Application",
                appath,
                FirewallAction.Allow,
                FirewallDirection.Outbound,
                FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public
            )
                {
                    Description = "LightVPN Application",
                    NetworkInterfaceTypes = NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan,
                    Protocol = FirewallProtocol.Any,
                    Name = "LightVPN"
                };
                FirewallWAS.Instance.Rules.Add(rule);
                FirewallWAS.Instance.Rules.Add(appRule);
            }
        }

        /// <summary>
        /// monitors the network
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(x => x.Name == _interfaceFriendlyName);
            foreach (var nic in interfaces)
                Execute(nic.OperationalStatus == OperationalStatus.Up);
        }
    }
}