using LightVPN.Windows;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Settingsv2.xaml
    /// </summary>
    public partial class Settings : Page, IDisposable
    {
        private MainWindow _host;

        public Settings(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            versionText.Text = $"LightVPN Windows Client [beta version {Assembly.GetEntryAssembly().GetName().Version}]";
        }

        public void Dispose()
        {
            _host = null;
            GC.SuppressFinalize(this);
        }

        private void BackToHome(object sender, RoutedEventArgs e) => _host.NavigatePage(new Main(_host));
    }
}