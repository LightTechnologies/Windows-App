/* --------------------------------------------
 *
 * Theme utilities - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: ThemeUtils.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Media;

namespace LightVPN
{
    /// <summary>
    /// Class that handles MaterialDesignInXaml theme changes
    /// </summary>
    public static class ThemeUtils
    {
        private static readonly FileLoggerBase logger = new ErrorLogger();

        /// <summary>
        /// Switches the theme via some resource dictionary hackery
        /// </summary>
        /// <param name="primaryColor">Primary color brush</param>
        /// <param name="secondaryColor">Secondary color brush</param>
        /// <param name="darkMode">Application theme</param>
        public static void SwitchTheme(string primaryColor, string secondaryColor, bool darkMode)
        {
            try
            {
                IBaseTheme baseTheme = darkMode ? Theme.Dark : Theme.Light;

                Color pColor = primaryColor == "Default" ? Color.FromRgb(147, 91, 249) : (Color)ColorConverter.ConvertFromString(primaryColor);
                Color sColor = primaryColor == "Default" ? Color.FromRgb(114, 124, 245) : (Color)ColorConverter.ConvertFromString(secondaryColor);

                ITheme theme = Theme.Create(baseTheme, pColor, sColor);

                ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, theme);
            }
            catch (Exception e)
            {
                logger.Write(e.ToString());
                MessageBox.Show("Something went wrong when processing theme settings, please report this exception, and the log file to LightVPN support.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
        }
    }
}