using System;
using System.Windows.Input;

namespace LightVPN.Client.Windows.Utils
{
    /// <inheritdoc />
    public sealed class UICommand : ICommand
    {
        public Action<object> CommandAction { get; init; }
        public Func<bool> CanExecuteFunc { get; init; }

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
