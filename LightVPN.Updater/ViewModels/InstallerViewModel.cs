using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.Updater.ViewModels
{
    public class InstallerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string statusText = "Connecting...";
        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private bool isIndeterminate = true;
        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        private int progressInt = -1;
        public int ProgressInt
        {
            get { return progressInt; }
            set
            {
                progressInt = value;
                OnPropertyChanged(nameof(ProgressInt));
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = async () =>
                    {
                        try
                        {
                            await Task.Delay(1000);

                            var path = Path.GetTempFileName();

                            var client = new HttpClientDownloadWithProgress("https://lightvpn.org/api/download/latest", path);

                            client.ProgressChanged += ProgressChanged;
                            ProgressInt = 0;
                            IsIndeterminate = false;
                            await client.StartDownload();
                            client.Dispose();
                            StatusText = "Installing...";
                            ProgressInt = -1;
                            IsIndeterminate = true;
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
                            Application.Current.MainWindow.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to update LightVPN, make sure LightVPN is not running or that another program is not using the LightVPN app files. Please report the following to support\n\n{ex}", "LightVPN", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            Application.Current.MainWindow.Close();
                        }
                    }
                };
            }
        }

        private void ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            var builder = new StringBuilder();
            builder.Append("Downloading... (");
            builder.Append(Math.Round(totalBytesDownloaded / 1024d / 1024d, 2));
            builder.Append(" MB / ");
            builder.Append(Math.Round(totalFileSize.Value / 1024d / 1024d, 2));
            builder.Append(" MB)");
            StatusText = builder.ToString();
            ProgressInt = (int)Math.Round(progressPercentage.Value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}