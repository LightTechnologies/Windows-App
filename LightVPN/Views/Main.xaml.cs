using LightVPN.Windows;
using System;
using System.Windows.Controls;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class Main : Page, IDisposable
    {
        private MainWindow _host;

        public Main(MainWindow host)
        {
            InitializeComponent();
            _host = host;
        }

        public void Dispose()
        {
            _host = null;
            GC.SuppressFinalize(this);
        }

        private void BeginningEdit(object sender, DataGridBeginningEditEventArgs e) => e.Cancel = true;
    }
}