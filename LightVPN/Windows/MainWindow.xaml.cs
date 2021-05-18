using Hardcodet.Wpf.TaskbarNotification;
using LightVPN.Common.Models;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using LightVPN.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LightVPN.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private readonly Main _mainView;

        private readonly TaskbarIcon _nofifyIcon;

        private readonly BeginStoryboard _viewLoaded;

        private readonly BeginStoryboard _viewUnloaded;

        public MainWindow()
        {
            InitializeComponent();

            // Fixes WPF's horrific maximize logic
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            var settings = Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().Load();

            if (settings.SizeSaving is not null && settings.SizeSaving.IsSavingSize)
            {
                this.Height = settings.SizeSaving.Height;
                this.Width = settings.SizeSaving.Width;
            }

            _viewLoaded = FindResource("LoadView") as BeginStoryboard;
            _viewUnloaded = FindResource("UnloadView") as BeginStoryboard;

            _mainView = new Main();

            NavigatePage(_mainView);

            // Just to initialize it
            _nofifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // Size saving handlers (can't do this in VM)
            this.SizeChanged += SizeChangedEvent;
        }

        public void Dispose()
        {
            _nofifyIcon.Dispose();
            if (Globals.Container.GetInstance<IManager>().IsConnected && !Globals.Container.GetInstance<IManager>().IsDisposed)
            {
                Globals.Container.GetInstance<IManager>().DisconnectAsync();
                Globals.Container.GetInstance<IManager>().Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public async void NavigatePage(Page page)
        {
            if (page is Main)
            {
                page = _mainView;
            }

            _viewUnloaded.Storyboard.Begin();
            await Task.Delay(400);
            MainFrame.Navigate(page);
        }

        private void MainWindowStateChanged(object sender, EventArgs e) => MaxIcon.Kind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;

        private void SettingsMenuItem(object sender, RoutedEventArgs e) => NavigatePage(new Views.Settings(this));

        private new async void SizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            var settings = await Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().LoadAsync();

            if (settings.SizeSaving is not null && settings.SizeSaving.IsSavingSize)
            {
                settings.SizeSaving.Height = (uint)Math.Round(this.Height);
                settings.SizeSaving.Width = (uint)Math.Round(this.Width);

                await Globals.Container.GetInstance<ISettingsManager<SettingsModel>>().SaveAsync(settings);
            }
        }

        private void UnloadCompleted(object sender, EventArgs e) => _viewLoaded.Storyboard.Begin();
    }
}