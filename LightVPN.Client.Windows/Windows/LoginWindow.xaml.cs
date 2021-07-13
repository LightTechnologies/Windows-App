namespace LightVPN.Client.Windows
{
    using System.Windows;
    using Common;

    /// <inheritdoc cref="System.Windows.Window" />
    internal sealed partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            this.InitializeComponent();

            Globals.LoginWindow = this;

            Application.Current.MainWindow = this;
        }
    }
}
