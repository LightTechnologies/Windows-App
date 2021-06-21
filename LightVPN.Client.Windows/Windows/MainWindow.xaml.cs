using LightVPN.Client.Windows.Views;
using System.Windows;
using System.Windows.Controls;

namespace LightVPN.Client.Windows
{
    /// <inheritdoc cref="System.Windows.Window" />
    internal sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow = this;

            LoadView(new MainView());
        }

        public void LoadView(Page page)
        {
            MainFrame.Navigate(page);
        }
    }
}