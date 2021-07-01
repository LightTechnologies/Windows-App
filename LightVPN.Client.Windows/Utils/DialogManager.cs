using System.Threading.Tasks;
using LightVPN.Client.Windows.Dialogs;
using LightVPN.Client.Windows.ViewModels;
using MaterialDesignThemes.Wpf;

namespace LightVPN.Client.Windows.Utils
{
    public static class DialogManager
    {
        public static async Task ShowDialogAsync(PackIconKind iconKind, string title, string message)
        {
            var view = new Dialog()
            {
                DataContext = new DialogViewModel(iconKind, title, message)
            };

            await DialogHost.Show(view, "RootDialog");
        }
    }
}
