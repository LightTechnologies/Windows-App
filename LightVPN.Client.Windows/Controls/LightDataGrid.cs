using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightVPN.Client.Windows.Controls
{
    internal sealed class LightDataGrid : DataGrid
    {
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(LightDataGrid),
                new UIPropertyMetadata());

        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(LightDataGrid),
                new UIPropertyMetadata());

        public LightDataGrid()
        {
            PreviewMouseDoubleClick += MouseButtonEventHandler;
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommand DoubleClickCommand
        {
            get => (ICommand)GetValue(DoubleClickCommandProperty);
            set => SetValue(DoubleClickCommandProperty, value);
        }

        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            DoubleClickCommand?.Execute(CommandParameter);
        }
    }
}