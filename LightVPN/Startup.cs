/* --------------------------------------------
 *
 * Startup class - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: Startup.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using DiscordRPC;
using Exceptionless;
using LightVPN.Auth;
using LightVPN.Auth.Interfaces;
using LightVPN.Common.Models;
using LightVPN.Discord;
using LightVPN.Discord.Interfaces;
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using LightVPN.OpenVPN;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings;
using LightVPN.Settings.Interfaces;
using LightVPN.Windows;
using SimpleInjector;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace LightVPN
{
    public class Startup : Application
    {
        internal static readonly FileLoggerBase logger = new ErrorLogger();

        public static LoginWindow LoginWindow { get; set; }

        [STAThread]
        public static void Main()
        {
            logger.Write($"LightVPN Windows Client [version {Assembly.GetEntryAssembly().GetName().Version}]");
            /* https://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c/229567 */

            string mutexId = string.Format("Global\\{{{0}}}", "f35bd589-5219-4668-9f78-b646442b1661");

            var allowEveryoneRule =
       new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                                                  , null)
                          , MutexRights.FullControl
                          , AccessControlType.Allow
                          );

            using var mutex = new Mutex(false, mutexId, out bool createdNew);

            var hasHandle = false;
            try
            {
                hasHandle = mutex.WaitOne(5000, false);
                if (hasHandle == false)
                {
                    logger.Write("(Startup/Main) Timeout waiting for exclusive access");
                    throw new TimeoutException("Timeout waiting for exclusive access");
                }

                ExceptionlessClient.Default.Register();
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = null,
                    UseProxy = false,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                    {
                        if (!cert.Issuer.ToLower().Contains("cloudflare") || error != System.Net.Security.SslPolicyErrors.None)
                        {
                            return false;
                        }
                        return true;
                    },
                };
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };
                Globals.container.Register(() => httpClientHandler, Lifestyle.Singleton);
                Globals.container.Register<IDiscordRpc, DiscordRpc>(Lifestyle.Singleton);
                Globals.container.Register(() => httpClient, Lifestyle.Singleton);
                Globals.container.Register<SSLCheckingHttpClient>(Lifestyle.Singleton);
                Globals.container.Register<IHttp, Http>(Lifestyle.Singleton);
                Globals.container.Register<ITapManager>(() => new TapManager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn", "tapctl.exe")), Lifestyle.Singleton);
                Globals.container.Register<IManager>(() => new Manager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn", "openvpn.exe")), Lifestyle.Singleton);
                Globals.container.Register(() => new DiscordRpcClient("833767448041226301"), Lifestyle.Singleton);
                Globals.container.Register<ISettingsManager<SettingsModel>>(() => new SettingsManager<SettingsModel>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "config.json")), Lifestyle.Singleton);

                Globals.container.Verify();
                logger.Write("(Startup/Main) Injected deps");

                // Starts a new resource dictionary
                var res = new ResourceDictionary();

                // Starts the WPF application and defines the params
                Startup a = new()
                {
                    StartupUri = new Uri("/Windows/LoginWindow.xaml", UriKind.RelativeOrAbsolute),
                    Resources = res
                };

                // Clears merged dictionaries
                res.MergedDictionaries.Clear();

                // Adds all the required resource dictionaries
                Uri uri = new("pack://application:,,,/LightVPN;component/Resources/Style.xaml", UriKind.RelativeOrAbsolute);
                Uri uri1 = new("pack://application:,,,/LightVPN;component/Resources/TrayStyle.xaml", UriKind.RelativeOrAbsolute);
                Uri mdUri = new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.RelativeOrAbsolute);
                Uri mdUri1 = new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.RelativeOrAbsolute);

                res.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                res.MergedDictionaries.Add(new ResourceDictionary() { Source = uri1 });
                res.MergedDictionaries.Add(new ResourceDictionary() { Source = mdUri });
                res.MergedDictionaries.Add(new ResourceDictionary() { Source = mdUri1 });

                logger.Write("(Startup/Main) Resources injected, executing app...");

                // Executes the app
                a.Run();

                return;
            }
            catch (TimeoutException)
            {
                MessageBox.Show("LightVPN is already running, check your system tray in the bottom right corner to find LightVPN's process", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
            catch (Exception e)
            {
                //if (MessageBox.Show($"An exception has occurred. Do you want to report the exception to the server? If no it will be written to the log file", "LightVPN", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                e.ToExceptionless().Submit();

                //else
                MessageBox.Show($"An exception has occurred. It has been written to the log file and uploaded to the server, please open an issue on GitHub with the log file attached so the developers can resolve the issue.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);

                logger.Write(e.ToString());
                Environment.Exit(0);
            }
            finally
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
        }
    }
}