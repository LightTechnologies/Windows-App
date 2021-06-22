using LightVPN.Client.Windows.Common;
using LightVPN.Client.Windows.Common.Utils;
using LightVPN.Client.Windows.Configuration.Interfaces;
using LightVPN.Client.Windows.Configuration.Models;
using LightVPN.Client.Windows.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightVPN.Client.Windows.ViewModels
{
    internal sealed class FirstRunViewModel : BaseViewModel
    {
        public ICommand ForwardCommand
        {
            get
            {
                return new UiCommand()
                {
                    CommandAction = _ =>
                    {
                        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();

                        var config = manager.Read();

                        config.IsFirstRun = false;

                        manager.Write(config);

                        var mainWindow = (MainWindow)Application.Current.MainWindow;
                        mainWindow?.LoadView(new MainView());
                    }
                };
            }
        }
    }
}
