namespace LightVPN.Client.Windows.ViewModels
{
    using System.Windows;
    using System.Windows.Input;
    using Common;
    using Configuration.Interfaces;
    using Configuration.Models;
    using Utils;
    using Views;

    internal sealed class FirstRunViewModel : BaseViewModel
    {
        public ICommand ForwardCommand
        {
            get
            {
                return new UICommand
                {
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();

                        var config = manager.Read();

                        config.IsFirstRun = false;

                        manager.Write(config);

                        var mainWindow = (MainWindow) Application.Current.MainWindow;
                        mainWindow?.LoadView(new MainView());
                    },
                };
            }
        }
    }
}
