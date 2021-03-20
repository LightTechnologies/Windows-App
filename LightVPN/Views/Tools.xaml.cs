/* --------------------------------------------
 * 
 * Tools view - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: Tools.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */

using LightVPN.Common.v2.Models;
using LightVPN.Interfaces;
using LightVPN.Logger;
using LightVPN.Logger.Base;
using LightVPN.OpenVPN.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Tools : Page
    {
        public static readonly DependencyProperty IsReinstallingTapProperty =
            DependencyProperty.Register("IsReinstallingTap", typeof(bool),
            typeof(Page), new(false));

        private readonly MainWindow _host;

        public bool IsReinstallingTap
        {
            get { return (bool)GetValue(IsReinstallingTapProperty); }
            set { SetValue(IsReinstallingTapProperty, value); }
        }

        public static readonly RoutedUICommand ReinstallTapCommand =
            new ("Reinstall",
                        "ReinstallTapCommand",
                        typeof(Tools));

        public static readonly RoutedUICommand ClearErrorLogCommand =
           new ("Clear",
                       "ClearErrorLogCommand",
                       typeof(Tools));

        public static readonly RoutedUICommand OpenErrorLogCommand =
           new ("Open",
                       "OpenErrorLogCommand",
                       typeof(Tools));

        public static readonly RoutedUICommand ClearOpenVpnLogCommand =
           new("Clear",
                       "ClearOpenVpnLogCommand",
                       typeof(Tools));

        private readonly FileLogger logger = new OpenVpnLogger(Globals.OpenVpnLogPath);

        private readonly FileLogger eLogger = new ErrorLogger(Globals.ErrorLogPath);

        public Tools(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            this.CommandBindings.Add(new CommandBinding(ReinstallTapCommand, ReinstallTapCommand_Event));
            this.CommandBindings.Add(new CommandBinding(ClearErrorLogCommand, ClearErrorLogCommand_Event));
            this.CommandBindings.Add(new CommandBinding(ClearOpenVpnLogCommand, ClearOpenVpnLogCommand_Event));
            this.CommandBindings.Add(new CommandBinding(OpenErrorLogCommand, OpenErrorLogCommand_Event));
            try
            {
                LogsOutput.Text = logger.Read();
            }
            catch (IOException e)
            {
                _host.ShowSnackbar($"{e.Message}");
            }
        }

        private void OpenErrorLogCommand_Event(object sender, ExecutedRoutedEventArgs args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = "notepad",
                    Arguments = Globals.ErrorLogPath,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };
            process.Start();
            process.Dispose();
        }

        private void ClearErrorLogCommand_Event(object sender, ExecutedRoutedEventArgs args)
        {
            eLogger.Clear();
            _host.ShowSnackbar("Cleared error logs");
        }

        private async void ClearOpenVpnLogCommand_Event(object sender, ExecutedRoutedEventArgs args)
        {
            logger.Clear();
            LogsOutput.Text = await logger.ReadAsync();
            _host.ShowSnackbar("Cleared OpenVPN logs");
        }

        private async void ReinstallTapCommand_Event(object sender, ExecutedRoutedEventArgs args)
        {
            IsReinstallingTap = true;
            await Task.Run(() =>
            {
                var instance = Globals.container.GetInstance<ITapManager>();
                if (instance.IsAdapterExistant())
                {
                    instance.RemoveTapAdapter();
                }
                instance.CreateTapAdapter();
            });
            _host.ShowSnackbar("Reinstalled the OpenVPN TAP adapter.");
            IsReinstallingTap = false;
        }
    }
}
