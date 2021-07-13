namespace LightVPN.Client.Windows.Utils
{
    using System.Threading.Tasks;
    using Dialogs;
    using MaterialDesignThemes.Wpf;
    using ViewModels;

    internal static class DialogManager
    {
        internal static async Task ShowDialogAsync(PackIconKind iconKind, string title, string message)
        {
            var view = new Dialog
            {
                DataContext = new DialogViewModel(iconKind, title, message),
            };

            await DialogHost.Show(view, "RootDialog");
        }
    }
}
