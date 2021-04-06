/* --------------------------------------------
 * 
 * Login window - (bad) Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: LoginWindow.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * Updated: 27-03-21 Khrysus
 *  NOTE: Added view models
 * 
 * --------------------------------------------
 */
using LightVPN.Auth.Interfaces;
using LightVPN.Auth.Models;
using LightVPN.Common.v2.Models;
using Exceptionless;
using LightVPN.Discord.Interfaces;
using LightVPN.Interfaces;
using LightVPN.Logger;
using LightVPN.Logger.Base;
using LightVPN.OpenVPN.Interfaces;
using LightVPN.Settings.Interfaces;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static LightVPN.Auth.ApiException;

namespace LightVPN.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static readonly RoutedUICommand SignInCommand =
        new ("Sign in",
                            "SignIn",
                            typeof(LoginWindow));

        private readonly Views.Login page = new ();

        private readonly FileLogger logger = new ErrorLogger(Globals.ErrorLogPath);

        private static bool _idk;
        public LoginWindow()
        {
            InitializeComponent();
            this.CommandBindings.Add(
            new CommandBinding(SignInCommand, LoginClick));
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            Globals.container.GetInstance<IThemeUtils>().SwitchTheme(new Auth.Models.Theme { DarkMode = settings.DarkMode, PrimaryColor = "Default", SecondaryColor = "Default" });
        }

        public void ShowSnackbar(string message)
        {
            var duration = 3;
            MainSnackbar.MessageQueue.Enqueue(
                $"{message}",
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(duration));
        }

        private async void LoginClick(object sender, ExecutedRoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(page.UsernameBox.Text) || string.IsNullOrWhiteSpace(page.PasswordBox.Password)) return;
                page.IsAuthenticating = true;
                page.SignInText.Text = " SIGNING IN...";
                var loginresponse = await Globals.container.GetInstance<IHttp>().LoginAsync(page.UsernameBox.Text, page.PasswordBox.Password);
                if (loginresponse.HasValue)
                {
                    Globals.OpenVpnUsername = loginresponse.Value.OpenVPNUsername;
                    Globals.OpenVpnPassword = loginresponse.Value.OpenVPNPassword;
                    page.SignInText.Text = " CHECKING FOR UPDATES...";
                    if (Version.TryParse(await Globals.container.GetInstance<IHttp>().GetVersionAsync(), out var version))
                    {
                        if (version > Assembly.GetEntryAssembly().GetName().Version)
                        {
                            Dispatcher.Invoke(() => page.SignInText.Text = " DOWNLOADING UPDATER...");
                            await Globals.container.GetInstance<IHttp>().GetUpdatesAsync();
                            return;
                        }
                    }
                    page.SignInText.Text = " CHECKING VPN DRIVER...";
                    if (!Globals.container.GetInstance<ITapManager>().CheckDriverExists())
                    {
                        page.SignInText.Text = " DOWNLOADING VPN DRIVER...";
                        await Globals.container.GetInstance<IHttp>().FetchOpenVpnDriversAsync();
                        page.SignInText.Text = " INSTALLING VPN DRIVER...";

                        await Task.Run(async () => await Globals.container.GetInstance<ITapManager>().InstallDriverAsync());
                    }

                    var encryption = Globals.container.GetInstance<IEncryption>();
                    await File.WriteAllTextAsync(Globals.AuthPath, encryption.Encrypt(JsonConvert.SerializeObject(new AuthFile { Username = page.UsernameBox.Text, Password = page.PasswordBox.Password })));
                    if (!await Globals.container.GetInstance<IHttp>().CachedConfigs())
                    {
                        page.SignInText.Text = " FETCHING SERVERS...";
                        await Task.Run(async () =>
                        {
                            await Globals.container.GetInstance<IHttp>().CacheConfigsAsync();
                        });
                    }
                    if (!await Globals.container.GetInstance<IHttp>().HasOpenVPN())
                    {
                        page.SignInText.Text = " FETCHING BINARIES...";
                        await Task.Run(async () =>
                        {
                            await Globals.container.GetInstance<IHttp>().GetOpenVPNBinariesAsync();
                        });
                    }
                    page.SignInText.Text = " CHECKING TAP ADAPTER";
                    if (!Globals.container.GetInstance<ITapManager>().IsAdapterExistant())
                    {
                        page.SignInText.Text = " INSTALLING TAP ADAPTER";
                        Globals.container.GetInstance<ITapManager>().CreateTapAdapter();
                    }
                    page.SignInText.Text = " LOADING UI...";
                    await Globals.container.GetInstance<IDiscordRpc>().SetPresenceObjectAsync(new DiscordRPC.RichPresence
                    {
                        State = "Disconnected"
                    });
                    if (string.IsNullOrWhiteSpace(loginresponse.Value.Email))
                    {
                        var emailpopup = new LightVPN.Views.EmailAlert();
                        NavigatePage(emailpopup);
                        await Task.Delay(10000);
                    }
                    await Globals.container.GetInstance<IDiscordRpc>().StartAsync();
                    MainWindow mw = new();
                    mw.Show();
                    this.Close();
                }
            }
            catch (SubscriptionExpiredException e)
            {
                ShowSnackbar(e.Message);
            }
            catch (ApiOfflineException e)
            {
                ShowSnackbar(e.Message);
            }
            catch (InvalidUsernameOrPasswordException e)
            {
                ShowSnackbar(e.Message);
                if (e.InnerException != null) logger.Write(e.InnerException.Message + "\n" + page.SignInBtn.Content);
            }
            catch (RatelimitedException)
            {
                ShowSnackbar("You have been ratelimited");
            }
            catch (InvalidResponseException e)
            {
                ShowSnackbar(e.Message);
                await logger.WriteAsync(e.ResponseString);
                if (e.InnerException != null)
                {
                    logger.Write(e.InnerException.Message);
                    e.ToExceptionless().SetUserIdentity(page.UsernameBox.Text).AddObject(page.SignInBtn.Content, "SignINText").Submit();
                }
            }
            catch (HttpRequestException e)
            {
                switch (e.Message)
                {
                    case "The SSL connection could not be established, see inner exception.":
                        ShowSnackbar("API certificate check failed.");
                        break;
                    default:
                        await logger.WriteAsync(e.ToString() + "\n" + page.SignInBtn.Content); e.ToExceptionless().SetUserIdentity(page.UsernameBox.Text).AddObject(page.SignInBtn.Content, "SignINText").Submit();
                        ShowSnackbar("Failed to send HTTP request to the LightVPN API.");
                        break;
                }
            }
            catch (Exception e)
            {
                await logger.WriteAsync(e.ToString());
                ShowSnackbar("Something went wrong.");
            }
            finally
            {
                page.SignInText.Text = " SIGN IN";
                page.IsAuthenticating = false;
            }
        }

        private void CaptionButtons_Click(object sender, RoutedEventArgs e)
        {
            //This is anti-smoothbrain, no cunt ever does this shit but I am a supreme programmer therefore I simplified it into one handler
            //Converts the sender object to a Button object using explicit casts ~ Khrysus
            Button button = (Button)sender;

            //Checks if the button content is a PackIcon and if so, runs the code below
            if (button.Content is PackIcon pI)
            {
                //Switches the PackIcon.Kind which is the icon shown as the PackIcon
                switch (pI.Kind)
                {
                    //Closes the window
                    case PackIconKind.WindowClose:
                        Environment.Exit(0);
                        break;

                    //Minimises the window
                    case PackIconKind.WindowMinimize:
                        WindowState = WindowState.Minimized;
                        break;
                }
            }
        }

        private void NavigatePage(Page @page)
        {
            MainFrame.Navigate(page);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigatePage(page);
            if (File.Exists(Globals.AuthPath))
            {
                var encryption = Globals.container.GetInstance<IEncryption>();
                var auth = JsonConvert.DeserializeObject<AuthFile>(encryption.Decrypt(File.ReadAllText(Globals.AuthPath)));
                page.UsernameBox.Text = auth.Username;
                page.PasswordBox.Password = auth.Password;
                if (!_idk)
                {
                    LoginClick(null, null);
                    _idk = true;
                }
            }
        }

        private bool ClosingAnimationFinished;

        private void FinishedClosingAnimation(object sender, EventArgs e)
        {
            ClosingAnimationFinished = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ClosingAnimationFinished)
            {
                BeginStoryboard sb = this.FindResource("CloseAnim") as BeginStoryboard;
                sb.Storyboard.Begin();
                e.Cancel = true;
            }
        }
    }
}