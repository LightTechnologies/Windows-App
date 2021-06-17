using System;
using System.Windows;
using LightVPN.Client.Windows.Models;
using LightVPN.Client.Windows.Utils;

namespace LightVPN.Client.Windows
{
    internal sealed class Program : Application
    {
        /// <summary>
        /// Main entry-point for the application, this is where it all begins.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            LightVPN.Client.Windows.Startup.Run();

            var res = new ResourceDictionary();

            // Starts the WPF application and defines the params
            Program a = new()
            {
                StartupUri = new Uri("Windows/MainWindow.xaml", UriKind.RelativeOrAbsolute),
                Resources = res
            };

            // Clears merged dictionaries
            res.MergedDictionaries.Clear();

            // Adds all the required resource dictionaries
            Uri mdUri = new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.RelativeOrAbsolute);
            Uri mdUri1 = new("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.RelativeOrAbsolute);

            res.MergedDictionaries.Add(new ResourceDictionary { Source = mdUri });
            res.MergedDictionaries.Add(new ResourceDictionary { Source = mdUri1 });

            ThemeManager.SwitchTheme(ThemeColor.Default, BackgroundMode.Light);

            a.Run();
        }
    }
}