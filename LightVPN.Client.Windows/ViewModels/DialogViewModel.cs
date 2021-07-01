using System.Drawing;
using MaterialDesignThemes.Wpf;

namespace LightVPN.Client.Windows.ViewModels
{
    internal class DialogViewModel : BaseViewModel
    {
        public DialogViewModel(PackIconKind icon, string title, string message)
        {
            Message = message;
            Title = title;
            IconKind = icon;
        }

        public string Message { get; }

        public string Title { get; }

        public PackIconKind IconKind { get; }

    }
}
