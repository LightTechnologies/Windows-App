using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightVPN.Controls
{
    /* Source adapted from https://stackoverflow.com/questions/3876662/wpf-datagrid-commandbinding-to-a-double-click-instead-of-using-events
     *
     * Modified to add command parameter
     *
     * MY FIRST CUSTOM WPF CONTROL WITH A COMMAND PARAMETER
     *
     */

    /// <summary>
    /// Custom data grid that has a mouse double click dependency property and a command parameter
    /// </summary>
    public class CustomDataGrid : DataGrid
    {
        /// <summary>
        /// Dependency property for the CommandParameter
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                    typeof(object),
                    typeof(CustomDataGrid),
                    new UIPropertyMetadata());

        // Using a DependencyProperty as the backing store for DoubleClickCommand. This enables
        // animation, styling, binding, etc...
        /// <summary>
        /// Dependency property for double click command
        /// </summary>
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand",
                typeof(ICommand),
                typeof(CustomDataGrid),
                new UIPropertyMetadata());

        /// <summary>
        /// Constructs the custom data grid
        /// </summary>
        public CustomDataGrid() : base() => PreviewMouseDoubleClick += new MouseButtonEventHandler(CustomDataGrid_PreviewMouseDoubleClick);
        /// <summary>
        /// Property for the CommandParameter
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        /// <summary>
        /// Property for the double click command
        /// </summary>
        public ICommand DoubleClickCommand
        {
            get { return (ICommand)GetValue(DoubleClickCommandProperty); }
            set { SetValue(DoubleClickCommandProperty, value); }
        }
        /// <summary>
        /// Handles and executes the ICommand for double clicking on the data grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomDataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickCommand is not null)
            {
                DoubleClickCommand.Execute(CommandParameter);
            }
        }
    }
}