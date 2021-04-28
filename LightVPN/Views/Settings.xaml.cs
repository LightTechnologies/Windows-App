using LightVPN.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Settingsv2.xaml
    /// </summary>
    public partial class Settings : Page
    {
        private readonly MainWindow _host;
        public Settings(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            versionText.Text = $"LightVPN Windows Client [beta version {Assembly.GetEntryAssembly().GetName().Version}]";
        }

        private void BackToHome(object sender, RoutedEventArgs e)
        {
            _host.NavigatePage(new Main(_host));
        }
    }
}
