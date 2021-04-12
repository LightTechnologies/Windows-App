using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LightVPN.Updater
{
    public class ThemeUtils
    {
        /// <summary>
        /// Switches the theme to the data provided in the color object
        /// </summary>
        public void SwitchTheme()
        {
                IBaseTheme baseTheme = Theme.Light;
                    ITheme defaultTheme = Theme.Create(baseTheme, Color.FromRgb(147, 91, 249), Color.FromRgb(114, 124, 245));
                    ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, defaultTheme);
                    return;
        }
    }
}
