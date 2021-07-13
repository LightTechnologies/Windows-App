namespace LightVPN.Client.Windows.ViewModels
{
    using MaterialDesignThemes.Wpf;

    internal sealed class DialogViewModel : BaseViewModel
    {
        public DialogViewModel(PackIconKind icon, string title, string message)
        {
            this.Message = message;
            this.Title = title;
            this.IconKind = icon;
        }

        public string Message { get; }

        public string Title { get; }

        public PackIconKind IconKind { get; }
    }
}
