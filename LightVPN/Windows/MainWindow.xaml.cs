using Hardcodet.Wpf.TaskbarNotification;
using LightVPN.Common.Models;
using LightVPN.OpenVPN;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightVPN.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BeginStoryboard _viewLoaded;
        private readonly BeginStoryboard _viewUnloaded;
        private readonly TaskbarIcon _notifyIcon;
        private Page _currentView;
        private readonly Main _mainView;
        private readonly IManager _manager;
        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            _viewLoaded = FindResource("LoadView") as BeginStoryboard;
            _viewUnloaded = FindResource("UnloadView") as BeginStoryboard;
            _mainView = new Main(this);
            _manager = Globals.container.GetInstance<IManager>();
            NavigatePage(_mainView);
            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        private void MainWindowStateChanged(object sender, EventArgs e)
        {
            //Handles Windows Aero maximize changes
            MaxIcon.Kind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
        }

        public async void NavigatePage(Page page)
        {
            if (_currentView?.Name != page.Name)
            {
                if (page is Main)
                {
                    page = _mainView;
                }

                _currentView = page;
                _viewUnloaded.Storyboard.Begin();
                await Task.Delay(400);
                MainFrame.Navigate(page);
            }
        }

        private void SettingsMenuItem(object sender, RoutedEventArgs e)
        {
            NavigatePage(new Views.Settings(this));
        }

        private void UnloadCompleted(object sender, EventArgs e)
        {
            _viewLoaded.Storyboard.Begin();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_manager.IsConnected)
            {
                _manager.Disconnect();
            }
        }
    }
}
