using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightVPN.Client.Windows.ViewModels
{
    /// <inheritdoc cref="System.ComponentModel.INotifyPropertyChanged" />
    internal abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly CancellationTokenSource CancellationTokenSource = new();

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}