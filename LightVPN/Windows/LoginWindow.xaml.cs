/* --------------------------------------------
 *
 * Login window - Model
 * Copyright (C) Light Technologies LLC
 *
 * File: LoginWindow.xaml.cs
 *
 * Created: 04-03-21 Khrysus
 * Updated: 27-03-21 Khrysus
 *  NOTE: Added view models
 *
 * --------------------------------------------
 */

using LightVPN.Common.Models;
using LightVPN.Settings.Interfaces;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace LightVPN.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            NavigatePage(new Views.Login());
            Startup.LoginWindow = this;
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            ThemeUtils.SwitchTheme("Default", "Default", settings.DarkMode);
        }

        private void CaptionButtons_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Content is PackIcon pI)
            {
                switch (pI.Kind)
                {
                    case PackIconKind.WindowClose:
                        Application.Current.Shutdown(); // CHANGE: Removed Environment.Exit for Application.Current.Shutdown for graceful exit.
                        break;

                    case PackIconKind.WindowMinimize:
                        WindowState = WindowState.Minimized;
                        break;
                }
            }
        }

        private void NavigatePage(Page page) => MainFrame.Navigate(page);
    }
}