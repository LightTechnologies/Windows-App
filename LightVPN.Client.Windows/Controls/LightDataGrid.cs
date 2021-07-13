namespace LightVPN.Client.Windows.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

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
            this.PreviewMouseDoubleClick += this.MouseButtonEventHandler;
        }

        public object CommandParameter
        {
            get => this.GetValue(LightDataGrid.CommandParameterProperty);
            set => this.SetValue(LightDataGrid.CommandParameterProperty, value);
        }

        public ICommand DoubleClickCommand
        {
            get => (ICommand) this.GetValue(LightDataGrid.DoubleClickCommandProperty);
            set => this.SetValue(LightDataGrid.DoubleClickCommandProperty, value);
        }

        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            this.DoubleClickCommand?.Execute(this.CommandParameter);
        }
    }
}
