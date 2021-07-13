namespace LightVPN.Client.Windows.Views
{
    using System.Windows.Controls;

    /// <inheritdoc cref="System.Windows.Controls.Page" />
    /// <summary>
    ///     Interaction logic for MainView.xaml
    /// </summary>
    internal sealed partial class MainView : Page
    {
        public MainView()
        {
            this.InitializeComponent();
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
