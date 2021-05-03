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
using LightVPN.Interfaces;
using LightVPN.Logger;
using LightVPN.Logger.Base;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Media;

namespace LightVPN
{
    public class ThemeUtils : IThemeUtils
    {
        private readonly FileLogger logger = new ErrorLogger();
        /// <summary>
        /// Switches the theme to the data provided in the color object
        /// </summary>
        /// <param name="colorObject">Color object which provides the data this method should use</param>
        public void SwitchTheme(Auth.Models.Theme colorObject)
        {
            try
            {
                IBaseTheme baseTheme = colorObject.DarkMode == true ? Theme.Dark : Theme.Light;
                if (colorObject.PrimaryColor == "Default")
                {
                    ITheme defaultTheme = Theme.Create(baseTheme, Color.FromRgb(147, 91, 249), Color.FromRgb(114, 124, 245));
                    ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, defaultTheme);
                    return;
                }
                Color pColor = (Color)ColorConverter.ConvertFromString(colorObject.PrimaryColor);
                Color sColor = (Color)ColorConverter.ConvertFromString(colorObject.SecondaryColor);
                ITheme theme = Theme.Create(baseTheme, pColor, sColor);
                ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, theme);
            }
            catch (Exception e)
            {
                logger.Write(e.ToString());
                MessageBox.Show("Something went wrong when processing your theme settings, please report this exception, and the log file to LightVPN support.", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }
        }
    }
}
