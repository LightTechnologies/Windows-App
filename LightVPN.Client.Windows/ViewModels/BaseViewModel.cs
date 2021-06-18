using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightVPN.Client.Windows.ViewModels
{
    /// <inheritdoc cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly CancellationTokenSource CancellationTokenSource = new();

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}