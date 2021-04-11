using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using CyberSniff.Updater;
using SharpCompress.Common;

namespace LightVPN.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = Path.GetTempFileName();

                var client = new HttpClientDownloadWithProgress("https://lightvpn.org/api/download/latest", path);

                client.ProgressChanged += Client_ProgressChanged;
                ProgressBar.Value = 0;
                ProgressBar.IsIndeterminate = false;
                await client.StartDownload();
                client.Dispose();
                //txtStatus.Text = "Installing...";
                //progressUpdate.Value = -1;
                //progressUpdate.IsIndeterminate = true;
                using (var archive = RarArchive.Open(path))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(Directory.GetCurrentDirectory(), new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
                File.Delete(path);
                Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "LightVPN.exe"));
                await Task.Delay(500);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update LightVPN, make sure LightVPN is not running or that another program is not using the LightVPN app files. Please report the following to support\n\n{ex}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
            }
        }
        private void Client_ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            var builder = new StringBuilder();
            builder.Append("Downloading... (");
            builder.Append(Math.Round(totalBytesDownloaded / 1024d / 1024d, 2));
            builder.Append(" MB / ");
            builder.Append(Math.Round(totalFileSize.Value / 1024d / 1024d, 2));
            builder.Append(" MB)");
            ProgressText.Text = builder.ToString();
            ProgressBar.Value = progressPercentage.Value;
        }
    }
}
