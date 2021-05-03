using System;
using System.Windows.Input;
using static LightVPN.ViewModels.MainViewModel;

namespace LightVPN
{
    internal class ConnectCommandArgs : ICommand
    {
        #region ICommand Members
        public Action<ServersModel> CommandAction { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (parameter is ServersModel serversModel)
            {
                CommandAction(serversModel);
            }
        }

        #endregion ICommand Members
    }
}