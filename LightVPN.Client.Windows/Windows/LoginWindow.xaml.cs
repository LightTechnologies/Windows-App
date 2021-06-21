using System.Windows;
using LightVPN.Client.Auth;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.ViewModels;

namespace LightVPN.Client.Windows
{
    /// <inheritdoc cref="System.Windows.Window" />
    internal sealed partial class LoginWindow : Window
    {
        internal LoginWindow()
        {
            InitializeComponent();

            Globals.LoginWindow = this;

            Application.Current.MainWindow = this;
        }
    }
}