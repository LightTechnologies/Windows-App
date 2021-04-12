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
using LightVPN.Common.v2.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

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

        public ICommand HideWindowCommand
        {
            get
            {
                return new TrayDelegate
                {
                    CommandAction = () =>
                    {
                        Globals.IsMinimizedToTray = true;
                        Application.Current.MainWindow.Hide();
                    },
                    CanExecuteFunc = () => Globals.IsMinimizedToTray == false
                };
            }
        }
    }
}