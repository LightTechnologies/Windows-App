/* --------------------------------------------
 * 
 * MainWindow MVVM
 * Copyright (C) Light Technologies LLC
 * 
 * File: MainWindowViewModel.cs
 * 
 * Created: 27-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Common.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
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

        public ICommand MinimizeWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                    }
                };
            }
        }

        public ICommand ToggleMaximizeWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                        }
                        else
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Maximized;
                        }
                    }
                };
            }
        }

        public ICommand HideWindowCommand
        {
            get
            {
                return new CommandDelegate
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