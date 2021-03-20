//using LightVPN.OpenVPN.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.NetworkInformation;
//using System.Text;
//using System.Threading.Tasks;
//using WindowsFirewallHelper;
//using WindowsFirewallHelper.FirewallRules;

//namespace LightVPN.OpenVPN.Classes
//{
//    public class Killswitch : IKillswitch
//    {
//        // https://github.com/t0nic/killswitch-core 
//        private readonly string _interfaceFriendlyName;
//        /// <summary>
//        /// Makes a new instance of the Killswitch 
//        /// </summary>
//        /// <param name="interfaceFriendlyName">The friendly name of the interface to monitor</param>
//        public Killswitch(string interfaceFriendlyName)
//        {
//            _interfaceFriendlyName = interfaceFriendlyName;
//        }
//        /// <summary>
//        /// Starts killswitch
//        /// </summary>
//        public void StartKillswitch()
//        {
//            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
//        }
//        /// <summary>
//        /// stops killswitch
//        /// </summary>
//        public void StopKillswitch()
//        {
//            NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
//        }
//        /// <summary>
//        /// monitors the network 
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
//        {
//            var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(x => x.Name == _interfaceFriendlyName);
//            foreach (var nic in interfaces)
//            {
//                if (nic.OperationalStatus == OperationalStatus.Up)
//                {
//                    Execute(true);
//                }
//                else
//                {
//                    Execute(false);
//                }
//            }
//        }
//        /// <summary>
//        /// executes the command to enable the rule and disable the rule
//        /// </summary>
//        /// <param name="enable"></param>
//        private static void Execute(bool enable)
//        {
//            var rule = FirewallWAS.Instance.Rules.FirstOrDefault(x => x.Description == "LightVPN Killswitch");
//            if (rule != null) { FirewallWAS.Instance.Rules.Remove(rule); rule = null; }
//            if (FirewallWAS.IsSupported && FirewallWASRuleWin8.IsSupported)
//            {
//                    rule ??= new FirewallWASRuleWin8(
//                    "Killswitch",
//                    enable ? FirewallAction.Allow : FirewallAction.Block,
//                    FirewallDirection.Outbound,
//                    FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public
//                )
//                {
//                    Description = "LightVPN Killswitch",
//                    NetworkInterfaceTypes = NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan,
//                    Protocol = FirewallProtocol.Any,
//                    Name = "Killswitch"
//                };
//                FirewallWAS.Instance.Rules.Add(rule);
//            }
//        }
//    }
//}
