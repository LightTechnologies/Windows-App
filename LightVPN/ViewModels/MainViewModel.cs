/* --------------------------------------------
 * 
 * MainWindow MVVM
 * Copyright (C) Light Technologies LLC
 * 
 * File: MainViewModel.cs
 * 
 * Created: 27-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightVPN.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
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