using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Utils;
using LightVPN.Client.Windows.Views;

namespace LightVPN.Client.Windows.ViewModels
{
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
                    }
                };
            }
        }
    }
}
