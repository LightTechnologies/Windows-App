using LightVPN.Common.v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class TrayIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new TrayDelegate
                {
                    CanExecuteFunc = () => Globals.IsMinimizedToTray == true,
                    CommandAction = () =>
                    {
                        Globals.IsMinimizedToTray = false;
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
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


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new TrayDelegate { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
}
