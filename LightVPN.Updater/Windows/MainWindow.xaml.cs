using System.Windows;
using System.Windows.Controls;
using LightVPN.Updater.Views;

namespace LightVPN.Updater.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            ThemeUtils.SwitchTheme();
            NavigatePage(new Changelog());
        }

        public void NavigatePage(Page page)
        {
            //_currentView = page;
            //_viewUnloaded.Storyboard.Begin();
            //await Task.Delay(400);
            MainFrame.Navigate(page);
        }
    }
}