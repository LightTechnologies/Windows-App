namespace LightVPN.Client.Windows.StartupHelper
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                Console.WriteLine(
                    $"LightVPN startup helper [version {Assembly.GetEntryAssembly()?.GetName().Version}]");
                Console.WriteLine("Copyright 2021 (C) Light Technologies, LLC.\n");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("More information: https://lightvpn.org/support/articles/startup-helper\n");
                Console.ResetColor();

                Console.WriteLine("[*] checking args");

                if (args.Length == 0 || !File.Exists(args.First()))
                    throw new InvalidOperationException("args are invalid, please pass in a valid file path");

                Console.WriteLine("[*] found binary\n[*] constructing process...");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = args.First(),
                        WorkingDirectory = Path.GetDirectoryName(args.First()) ??
                                           throw new ArgumentNullException(nameof(args)),
                        Verb = "runas",
                        Arguments = "--minimised",
                        UseShellExecute = true,
                    },
                };

                Console.WriteLine("[*] executing binary...");

                process.Start();

                Console.WriteLine($"[*] started pid {process.Id} (sessId: {process.SessionId})\n[*] exiting...");

                return 0;
            }
            catch (InvalidOperationException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    $"[!] {e.Message}");
                return 1;
            }
            catch (Win32Exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    "[!] a win32 exception has been thrown, this could be due to pressing 'No' on the user account control dialog. as a result lightvpn will not start this time.");
                return 2;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    $"[!] an exception has been thrown!\n\n[!] please head over to https://github.com/lighttechnologies/windows-app and open an issue containing the following:\n\n{e}\n\n[*] exiting...");
                return 3;
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
