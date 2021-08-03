namespace LightVPN.Client.Windows
{
    using System;
    using System.Linq;
    using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
    using System.Windows;
    using Auth;
    using Auth.Interfaces;
    using Auth.Models;
    using Common;
    using Common.Utils;
    using Configuration;
    using Configuration.Interfaces;
    using Configuration.Models;
    using Debug;
    using Discord;
    using Discord.Interfaces;
    using Discord.Models;
    using Models;
    using OpenVPN;
    using OpenVPN.Interfaces;
    using OpenVPN.Models;
    using Services;
    using Services.Interfaces;
    using Utils;

    /// <inheritdoc />
    /// <summary>
    ///     Handles the injection of services
    /// </summary>
    internal sealed class Startup : Application
    {
        /// <summary>
        ///     Creates a new service collection and configures all the services (this does not startup the WPF UI)
        /// </summary>
        [STAThread]
        internal static void Main(string[] args)
        {
            string mutexId = string.Format("Global\\{{{0}}}", "f35bd589-5219-4668-9f78-b646442b1661");

            var allowEveryoneRule =
       new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                                                  , null)
                          , MutexRights.FullControl
                          , AccessControlType.Allow
                          );
            using var mutex = new Mutex(false, mutexId, out _);

            var hasHandle = false;
            try
            {
                hasHandle = mutex.WaitOne(5000, false);
                if (hasHandle == false)
                {
                    throw new TimeoutException("Timeout waiting for exclusive access");
                }

                DebugLogger.Write("lvpn-client-win-ep",
                $"app: {Assembly.GetEntryAssembly()?.GetName().Version}, host os: {HostVersion.GetOsVersion()}");

                DebugLogger.Write("lvpn-client-win-ep", $"args len: {args.Length}");

                if (args.Length != 0)
                {
                    DebugLogger.Write("lvpn-client-win-ep", "parsing args");
                    switch (args.FirstOrDefault())
                    {
                        case "--minimised":
                            Globals.IsStartingMinimised = true;
                            DebugLogger.Write("lvpn-client-win-ep", "min arg, 1");
                            break;
                        default:
                            DebugLogger.Write("lvpn-client-win-ep", "unrecognised args, ignoring...");
                            break;
                    }
                }

                var ovpnConf = new OpenVpnConfiguration
                {
                    OpenVpnLogPath = Globals.OpenVpnLogPath,
                    OpenVpnPath = Globals.OpenVpnPath,
                    TapAdapterName = "LightVPN-TAP",
                    TapCtlPath = Globals.TapCtlPath,
                };

                Globals.Container.RegisterSingleton<IApiClient, ApiClient>();
                Globals.Container.RegisterSingleton<ICacheService, CacheService>();
                Globals.Container.RegisterInstance<IVpnManager>(new VpnManager(ovpnConf));
                Globals.Container.RegisterInstance<IDiscordRp>(new DiscordRp(new DiscordRpConfiguration
                {
                    ClientId = 856714133629829130,
                    LargeImageKey = "lvpn",
                    LargeImageText = $"v{Assembly.GetEntryAssembly()?.GetName().Version} ({HostVersion.GetOsVersion()})",
                }));
                Globals.Container.RegisterInstance<IConfigurationManager<AppConfiguration>>(
                    new ConfigurationManager<AppConfiguration>(Globals.AppSettingsPath));
                Globals.Container.RegisterSingleton<IOpenVpnService, OpenVpnService>();

                DebugLogger.Write("lvpn-client-win-ep", "registered service instances");

                var res = new ResourceDictionary();
                res.MergedDictionaries.Clear();

                var app = new Startup
                {
                    Resources = res,
                    StartupUri = new Uri("Windows/LoginWindow.xaml", UriKind.RelativeOrAbsolute),
                };

                // Adds all the required resource dictionaries

                // Resource dictionary containing all of the color schemes
                Uri colorsUri =
                    new("Resources/Colors.xaml", UriKind
                        .RelativeOrAbsolute);

                // Resource dictionary containing all FontFamilys
                Uri fontsUri =
                    new("Resources/Fonts.xaml", UriKind
                        .RelativeOrAbsolute);

                // Resource dictionary containing all styles of text (title, subtitle, etc.)
                Uri typographyUri = new("Resources/Typography.xaml", UriKind
                    .RelativeOrAbsolute);

                // Resource dictionary containing styles for buttons
                Uri buttonsUri =
                    new("Resources/Buttons.xaml", UriKind
                        .RelativeOrAbsolute);

                // Resource dictionary containing window/view specific styles
                Uri windowsUri =
                    new("Resources/Windows.xaml", UriKind
                        .RelativeOrAbsolute);

                // Resource dictionary to do with the tray icon
                Uri trayUri =
                    new("Resources/Tray.xaml", UriKind
                        .RelativeOrAbsolute);

                // Material design in XAML toolkit resource dictionaries
                Uri mdUri =
                    new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"
                        , UriKind.RelativeOrAbsolute);
                Uri mdUri1 =
                    new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml"
                        , UriKind.RelativeOrAbsolute);

                res.MergedDictionaries.Add(new ResourceDictionary { Source = mdUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = mdUri1, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = colorsUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = fontsUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = typographyUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = buttonsUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = windowsUri, });
                res.MergedDictionaries.Add(new ResourceDictionary { Source = trayUri, });

                DebugLogger.Write("lvpn-client-win-ep", "attempting to authenticate...");

                try
                {
                    var settings = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read();
                    if (settings is not null)
                        ThemeManager.SwitchTheme(ThemeColor.Default,
                            settings.IsDarkModeEnabled ? BackgroundMode.Dark : BackgroundMode.Light);
                    else
                        ThemeManager.SwitchTheme(ThemeColor.Default, BackgroundMode.Light);

                    Globals.Container.GetInstance<IApiClient>().GetAsync<GenericResponse>("profile").GetAwaiter()
                        .GetResult();

                    if (settings is { IsDiscordRpcEnabled: true, }) Globals.Container.GetInstance<IDiscordRp>().Initialize();

                    DebugLogger.Write("lvpn-client-win-ep", "auth success");
                    app.StartupUri = new Uri("Windows/MainWindow.xaml", UriKind.RelativeOrAbsolute);
                }
                catch (Exception)
                {
                    DebugLogger.Write("lvpn-client-win-ep", "auth failed");
                    // Apply default theme settings
                    ThemeManager.SwitchTheme(ThemeColor.Default, BackgroundMode.Light);
                }

                DebugLogger.Write("lvpn-client-win-ep", "attempt cache ovpn");
                Globals.Container.GetInstance<ICacheService>().CacheOpenVpnBinariesAsync();

                DebugLogger.Write("lvpn-client-win-ep", "starting app instance");

                app.Run();
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
        }
    }
}
