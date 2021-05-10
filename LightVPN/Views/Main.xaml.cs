using System.Windows.Controls;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        private void BeginningEdit(object sender, DataGridBeginningEditEventArgs e) => e.Cancel = true;
    }
}