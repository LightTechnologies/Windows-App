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
using LightVPN.Discord.Interfaces;
using LightVPN.Interfaces;
using LightVPN.OpenVPN;
using LightVPN.Discord;
using LightVPN.OpenVPN.Interfaces;
using SimpleInjector;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using LightVPN.Common.v2.Models;
using LightVPN.Auth.Interfaces;
using LightVPN.Auth;
using System.Net.Http;
using LightVPN.Logger;
using LightVPN.Logger.Base;
using LightVPN.Settings.Interfaces;
using LightVPN.Settings;
using LightVPN.Common.v2.Interfaces;
using System.Diagnostics;
using System.Reflection;
using Exceptionless;
using LightVPN.Auth.Classes;
using System.Net.Http.Headers;
using LightVPN.Common.v2;
using LightVPN.OpenVPN.Classes;

namespace LightVPN
{
    /*
     * Hi HoverCore!
     * Just to let you know, John licensed this source code under the Light Technologies Open Source license
     * That means that since LightVPN is a trademark of Light Technologies LLC, a registered company in the US (pending)
     * We can sue the living shit out of you, so just warning you not to steal any of our code
     * Also you don't know shit and you never will.
     * 
     * - Khrysus (also I did the UI & the dependency injection suck my dick)
     *   LightVPN Client UX, UI & website developer.
     */
    public class Startup : Application
    {
        internal static readonly FileLogger logger = new ErrorLogger(Globals.ErrorLogPath);

        private static readonly Mutex mutex = new(true, "{92867d36-e58d-43ef-bdd2-7649fa38be5f}", out isOnlyInstance);

        private static bool isOnlyInstance = false;

        [STAThread]
        public static void Main()
        {
            ExceptionlessClient.Default.Register();
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = null,
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                {
                    if (!cert.Issuer.ToLower().Contains("let's encrypt") || error != System.Net.Security.SslPolicyErrors.None)
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
            Globals.container.Register<IEncryption, Encryption>(Lifestyle.Singleton);
            Globals.container.Register(() => httpClientHandler, Lifestyle.Singleton);
            Globals.container.Register<IThemeUtils, ThemeUtils>(Lifestyle.Singleton);
            Globals.container.Register<IKillswitch>(() => new Killswitch("LightVPN-TAP"), Lifestyle.Singleton);
            Globals.container.Register<IDiscordRpc, DiscordRpc>(Lifestyle.Singleton);
            Globals.container.Register<INative, Native>(Lifestyle.Singleton);
            Globals.container.Register(() => httpClient, Lifestyle.Singleton);
            Globals.container.Register<SSLCheckingHttpClient>(Lifestyle.Singleton);
            Globals.container.Register<IHttp, Http>(Lifestyle.Singleton);
            Globals.container.Register<ITapManager>(() => new TapManager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn", "tapctl.exe")), Lifestyle.Singleton);
            Globals.container.Register<IManager>(() => new Manager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "ovpn", "openvpn.exe")), Lifestyle.Singleton);
            Globals.container.Register(() => new DiscordRpcClient("833767448041226301"), Lifestyle.Singleton);
            Globals.container.Register<ISettingsManager<SettingsModel>>(() => new SettingsManager<SettingsModel>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightVPN", "config.json")), Lifestyle.Singleton);

            Globals.container.Verify();
            try
            {
                // Check if mutex already exists
                if (isOnlyInstance)
                {
                    MessageBox.Show($"LightVPN is already running or not responding.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

                // Executes the app
                a.Run();

                // Keeps the mutex alive whilst the app is
                GC.KeepAlive(mutex);

                return;
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
        }
    }
}
