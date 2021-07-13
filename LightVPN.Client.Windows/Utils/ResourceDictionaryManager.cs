namespace LightVPN.Client.Windows.Utils
{
    using System;
    using System.Windows;

    /// <summary>
    ///     Generates resource dictionaries, must be called after startup object construction
    /// </summary>
    public static class ResourceDictionaryManager
    {
        /// <summary>
        ///     Generates a resource dictionary containing all the merged dictionaries
        /// </summary>
        /// <returns>The newly generated dictionary</returns>
        public static ResourceDictionary GenerateResourceDictionary()
        {
            var res = new ResourceDictionary();
            res.MergedDictionaries.Clear();

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

            res.MergedDictionaries.Add(new ResourceDictionary {Source = mdUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = mdUri1,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = colorsUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = fontsUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = typographyUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = buttonsUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = windowsUri,});
            res.MergedDictionaries.Add(new ResourceDictionary {Source = trayUri,});

            return res;
        }
    }
}
