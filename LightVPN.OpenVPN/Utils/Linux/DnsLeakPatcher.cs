using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN.Utils.Linux
{
    public static class DnsLeakPatcher
    {
        public static bool IsDnsLeaksPatched()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Checking if DNS leaks have been patched...");

            var networkManagerConfiguration = File.ReadAllText("/etc/NetworkManager/NetworkManager.conf");
            var resolvdConfiguration = File.ReadAllText("/etc/resolv.conf");

            return networkManagerConfiguration.Contains("dns=dnsmasq") && resolvdConfiguration.Contains("nameserver 127.0.0.1");
        }

        public static async Task PatchDnsLeaksAsync()
        {
            /* Commands adapted from https://askubuntu.com/questions/1065568/block-outside-dns-fix-dns-leak-ubuntu-18-04 */

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Patching your system from DNS leaks...");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Disabling resolved...");
            // Disables the system DNS resolver
            var proc = new Process
            {
                StartInfo = new()
                {
                    Arguments = "disable systemd-resolved.service",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "systemctl",
                    Verb = "runas"
                }
            };

            proc.Start();
            await proc.WaitForExitAsync();

            // Stops the system DNS resolver
            proc = new Process
            {
                StartInfo = new()
                {
                    Arguments = "stop systemd-resolved.service",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "systemctl",
                    Verb = "runas"
                }
            };

            proc.Start();
            await proc.WaitForExitAsync();

            proc.Dispose();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Backing up resolvd & NetworkManager configurations...");

            File.Copy("/etc/resolv.conf", "/etc/resolv.conf.bak");
            File.Copy("/etc/NetworkManager/NetworkManager.conf", "/etc/NetworkManager/NetworkManager.conf.bak");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Configuring resolvd...");

            // Deletes the existing DNS resolver config
            File.Delete("/etc/resolv.conf");

            // Replaces it with a fixed configuration
            await File.WriteAllTextAsync("/etc/resolv.conf", "nameserver 127.0.0.1");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[-] Configuring NetworkManager...");

            bool found = false;
            List<string> lines = new();
            foreach (var line in await File.ReadAllLinesAsync("/etc/NetworkManager/NetworkManager.conf"))
            {
                lines.Add(line);
                if (line.Contains("[main]"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[!] Found [main] at index {lines.IndexOf(line)}");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[-] Writing configuration...");
                await File.AppendAllTextAsync("/etc/NetworkManager/NetworkManager.conf", @"[main]
plugins=ifupdown, keyfile
dns=dnsmasq");
            }
            else
            {
                Console.WriteLine($"[-] Attempting to insert configuration...");
                var line = lines.FirstOrDefault(x => x.Contains("[main]"));
                int lineIndex = lines.IndexOf(line);
                lines.Insert(lineIndex, "dns=dnsmasq");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] DNSMASQ patched");
                lines.Insert(lineIndex, "plugins=ifupdown, keyfile");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] Plugins patched");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[-] Writing configuration...");
                await File.WriteAllLinesAsync("/etc/NetworkManager/NetworkManager.conf", lines);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] DNS leaks have been patched, you will need to reboot the system for the new configuration to take place!");
        }
    }
}
