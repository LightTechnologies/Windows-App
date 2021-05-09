using System;
using System.Windows.Input;

namespace LightVPN.Updater
{
    internal class CommandDelegate : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Func<bool> CanExecuteFunc { get; set; }

        public Action CommandAction { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public void Execute(object parameter)
        {
            CommandAction();
        }
    }
}