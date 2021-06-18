using System;
using System.Windows.Input;

namespace LightVPN.Client.Windows.Common.Utils
{
    /// <inheritdoc />
    public sealed class UiCommand : ICommand
    {
        public Action<object> CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object args)
        {
            CommandAction(args);
        }

        public bool CanExecute(object args)
        {
            return CanExecuteFunc?.Invoke() != false;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}