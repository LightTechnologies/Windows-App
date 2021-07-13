namespace LightVPN.Client.Windows.Utils
{
    using System;
    using System.Windows.Input;

    /// <inheritdoc />
    internal sealed class UICommand : ICommand
    {
        internal Action<object> CommandAction { get; init; }
        internal Func<bool> CanExecuteFunc { get; init; }

        public void Execute(object args)
        {
            this.CommandAction(args);
        }

        public bool CanExecute(object args)
        {
            return this.CanExecuteFunc?.Invoke() != false;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
