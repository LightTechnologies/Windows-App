namespace LightVPN.Client.Windows.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <inheritdoc cref="System.ComponentModel.INotifyPropertyChanged" />
    internal abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly CancellationTokenSource CancellationTokenSource = new();

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isPlayingAnimation;

        public bool IsPlayingAnimation
        {
            get => this._isPlayingAnimation;
            set
            {
                this._isPlayingAnimation = value;
                this.OnPropertyChanged(nameof(BaseViewModel.IsPlayingAnimation));
            }
        }

        public void Dispose()
        {
            this.IsPlayingAnimation = false;
            this.CancellationTokenSource.Cancel();
        }
    }
}
