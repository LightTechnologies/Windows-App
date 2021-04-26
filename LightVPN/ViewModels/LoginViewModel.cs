using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightVPN.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private bool isAuthenticating;
        public bool IsAuthenticating
        {
            get { return isAuthenticating; }
            set
            {
                isAuthenticating = value;
                OnPropertyChanged(nameof(IsAuthenticating));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
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