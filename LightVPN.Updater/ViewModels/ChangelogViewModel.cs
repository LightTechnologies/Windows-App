using LightVPN.Auth;
using LightVPN.Updater.Views;
using LightVPN.Updater.Windows;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.Updater.ViewModels
{
    public class ChangelogViewModel : INotifyPropertyChanged
    {
        private readonly Timer _timer = new();

        private string changelog;

        private int elapsedSw = 30;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Changelog
        {
            get { return changelog; }

            set
            {
                changelog = value;
                OnPropertyChanged(nameof(Changelog));
            }
        }

        public int ElapsedSw
        {
            get { return elapsedSw; }

            set
            {
                elapsedSw = value;
                OnPropertyChanged(nameof(ElapsedSw));
            }
        }

        public ICommand LoadCommand => new CommandDelegate()
        {
            CommandAction = async () =>
            {
                _timer.Elapsed += Elapsed;
                _timer.Interval = 1000;
                _timer.Start();

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = null,
                    UseProxy = false,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                    {
                        if (!cert.Issuer.ToLower().Contains("cloudflare") || error != System.Net.Security.SslPolicyErrors.None)
                        {
                            return false;
                        }
                        return true;
                    },
                };

                var auth = new Http(new ApiHttpClient(httpClientHandler), System.PlatformID.Win32NT);

                Changelog = await auth.GetChangelogAsync();
            }
        };

        public ICommand NextPageCommand
        {
            get
            {
                return new CommandDelegate()
                {
                    CommandAction = () =>
                    {
                        _timer.Stop();
                        var window = (MainWindow)Application.Current.MainWindow;
                        window.NavigatePage(new Installer());
                    }
                };
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ElapsedSw > 0)
            {
                ElapsedSw -= 1;
            }
            else
            {
                _timer.Stop();

                MainWindow mainWindow = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow.NavigatePage(new Installer());
                });
            }
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