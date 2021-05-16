using System;
using System.Windows.Input;

namespace LightVPN.Delegates
{
    /// <summary>
    /// The base command delegate used with every single ICommand in this program
    /// </summary>
    public class CommandDelegate : ICommand
    {
        /// <summary>
        /// The code that will be executed when this command is called
        /// </summary>
        public Action<object> CommandAction { get; set; }
        /// <summary>
        /// Basic validation, if the value is true then you can run the function, otherwise you can't. This also changes the button state
        /// </summary>
        public Func<bool> CanExecuteFunc { get; set; }
        /// <summary>
        /// Executes the commmand with the specified parameter
        /// </summary>
        /// <param name="parameter">The parameter to be passed to the command action</param>
        public void Execute(object parameter) => CommandAction(parameter);
        /// <summary>
        /// Checks if the command can be executed or not
        /// </summary>
        /// <param name="parameter">The parameter, this is not used</param>
        /// <returns></returns>
        public bool CanExecute(object parameter) => CanExecuteFunc == null || CanExecuteFunc();
        /// <summary>
        /// The event that is raised when the execution state is changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
