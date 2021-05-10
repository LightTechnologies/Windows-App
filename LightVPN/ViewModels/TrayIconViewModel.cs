using LightVPN.Common.Models;
using LightVPN.Delegates;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.ViewModels
{
    public class TrayIconViewModel
    {
        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public static ICommand ExitApplicationCommand
        {
            get
            {
                return new CommandDelegate { CommandAction = (args) => Application.Current.Shutdown() };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public static ICommand HideWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CommandAction = (args) =>
                    {
                        Globals.IsMinimizedToTray = true;
                        Application.Current.MainWindow.Hide();
                    },
                    CanExecuteFunc = () => Globals.IsMinimizedToTray == false
                };
            }
        }

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public static ICommand ShowWindowCommand
        {
            get
            {
                return new CommandDelegate
                {
                    CanExecuteFunc = () => Globals.IsMinimizedToTray == true,
                    CommandAction = (args) =>
                    {
                        Globals.IsMinimizedToTray = false;
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }
    }
}