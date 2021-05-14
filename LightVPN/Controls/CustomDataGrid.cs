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

    public class CustomDataGrid : DataGrid
    {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                    typeof(object),
                    typeof(CustomDataGrid),
                    new UIPropertyMetadata());

        // Using a DependencyProperty as the backing store for DoubleClickCommand. This enables
        // animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand",
                typeof(ICommand),
                typeof(CustomDataGrid),
                new UIPropertyMetadata());

        public CustomDataGrid() : base() => this.PreviewMouseDoubleClick += new MouseButtonEventHandler(CustomDataGrid_PreviewMouseDoubleClick);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public ICommand DoubleClickCommand
        {
            get { return (ICommand)GetValue(DoubleClickCommandProperty); }
            set { SetValue(DoubleClickCommandProperty, value); }
        }

        private void CustomDataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickCommand == null) return;
            DoubleClickCommand.Execute(CommandParameter);
        }
    }
}