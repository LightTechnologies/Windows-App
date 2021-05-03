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
using LightVPN.Common.Models;
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
using LightVPN.Settings.Exceptions;

namespace LightVPN.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        public LoginWindow()
        {
            InitializeComponent();
            NavigatePage(new Views.Login());
            Startup.LoginWindow = this;
            var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
            Globals.container.GetInstance<IThemeUtils>().SwitchTheme(new Auth.Models.Theme { DarkMode = settings.DarkMode, PrimaryColor = "Default", SecondaryColor = "Default" });
        }

        /* Toshi is bad at C# nigga I was tired and didn't give a shit I was planning on changing it */
        //internal async Task ProcessLoginAsync(bool isSessionAuth = false, Guid sessionId = default)
        //{
        //    var settings = Globals.container.GetInstance<ISettingsManager<SettingsModel>>().Load();
        //    try
        //    {
        //        var validationResult = Validation.GetErrors(page.UsernameBox);
        //        if (string.IsNullOrWhiteSpace(page.UsernameBox.Text) || string.IsNullOrWhiteSpace(page.PasswordBox.Password) || validationResult.Count > 0) return;
        //        page.IsAuthenticating = true;
        //        page.SignInText.Text = "SIGNING IN...";
        //        AuthResponse authResponse = new();
        //        if (isSessionAuth)
        //        {
        //            var loginResponse = await Globals.container.GetInstance<IHttp>().ValidateSessionAsync(page.UsernameBox.Text, sessionId);
        //            if (!loginResponse)
        //            {
        //                ShowSnackbar("Your session is invalid, please sign in again.");
        //                page.SignInText.Text = "SIGN IN";
        //                page.IsAuthenticating = false;
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            authResponse = await Globals.container.GetInstance<IHttp>().LoginAsync(page.UsernameBox.Text, page.PasswordBox.Password);
        //        }

        //        page.SignInText.Text = "CHECKING VPN DRIVER...";
        //        if (!Globals.container.GetInstance<ITapManager>().CheckDriverExists())
        //        {
        //            page.SignInText.Text = "DOWNLOADING VPN DRIVER...";
        //            await Globals.container.GetInstance<IHttp>().FetchOpenVpnDriversAsync();
        //            page.SignInText.Text = "INSTALLING VPN DRIVER...";

        //            await Task.Run(async () => await Globals.container.GetInstance<ITapManager>().InstallDriverAsync());
        //        }

        //        if (!isSessionAuth)
        //        {
        //            var encryption = Globals.container.GetInstance<IEncryption>();
        //            await File.WriteAllTextAsync(Globals.AuthPath, encryption.Encrypt(JsonConvert.SerializeObject(new AuthFile { Username = page.UsernameBox.Text, Password = page.PasswordBox.Password, SessionId = authResponse.Session })));
        //        }
        //        if (!await Globals.container.GetInstance<IHttp>().IsConfigsCachedAsync())
        //        {
        //            page.SignInText.Text = "FETCHING SERVERS...";
        //            await Task.Run(async () =>
        //            {
        //                await Globals.container.GetInstance<IHttp>().CacheConfigsAsync();
        //            });
        //        }
        //        if (!await Globals.container.GetInstance<IHttp>().HasOpenVpnAsync())
        //        {
        //            page.SignInText.Text = "FETCHING BINARIES...";
        //            await Task.Run(async () =>
        //            {
        //                await Globals.container.GetInstance<IHttp>().GetOpenVpnBinariesAsync();
        //            });
        //        }
        //        page.SignInText.Text = "CHECKING TAP ADAPTER...";
        //        if (!Globals.container.GetInstance<ITapManager>().IsAdapterExistant())
        //        {
        //            page.SignInText.Text = "INSTALLING TAP ADAPTER...";
        //            Globals.container.GetInstance<ITapManager>().CreateTapAdapter();
        //        }
        //        page.SignInText.Text = "LOADING UI...";
        //        if (settings.DiscordRpc)
        //        {
        //            Globals.container.GetInstance<IDiscordRpc>().Initialize();
        //        }
        //        Application.Current.MainWindow = new MainWindow();
        //        Application.Current.MainWindow.Show();
        //        this.Close();
        //    }
        //    catch (ClientUpdateRequired)
        //    {
        //        Dispatcher.Invoke(() => page.SignInText.Text = "DOWNLOADING UPDATER...");
        //        await Globals.container.GetInstance<IHttp>().GetUpdatesAsync();
        //    }
        //    catch (InvalidResponseException e)
        //    {
        //        ShowSnackbar(e.Message);
        //        await logger.WriteAsync(e.ResponseString);
        //        if (e.InnerException != null)
        //        {
        //            logger.Write(e.InnerException.Message);
        //            e.ToExceptionless().SetUserIdentity(page.UsernameBox.Text).AddObject(page.SignInBtn.Content, "SignInText").Submit();
        //        }
        //    }
        //    catch (RatelimitedException e)
        //    {
        //        ShowSnackbar(e.Message);
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        switch (e.Message)
        //        {
        //            case "The SSL connection could not be established, see inner exception.":
        //                ShowSnackbar("API certificate check failed.");
        //                break;
        //            default:
        //                await logger.WriteAsync(e.ToString() + "\n" + page.SignInBtn.Content); e.ToExceptionless().SetUserIdentity(page.UsernameBox.Text).AddObject(page.SignInBtn.Content, "SignINText").Submit();
        //                ShowSnackbar("Failed to send HTTP request to the LightVPN API.");
        //                break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        await logger.WriteAsync(e.ToString());
        //        ShowSnackbar("Something went wrong, check the error log for more info.");
        //    }
        //    finally
        //    {
        //        page.SignInText.Text = "SIGN IN";
        //        page.IsAuthenticating = false;
        //    }
        //}

        private void CaptionButtons_Click(object sender, RoutedEventArgs e)
        {
            //This is anti-smoothbrain, no cunt ever does this shit but I am a supreme programmer therefore I simplified it into one handler
            //Converts the sender object to a Button object using explicit casts ~ Khrysus
            Button button = (Button)sender;

            //Checks if the button content is a PackIcon and if so, runs the code below
            if (button.Content is PackIcon pI)
            {
                //Switches the PackIcon.Kind which is the icon shown on the button
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

        //private async void Window_Loaded(object sender, RoutedEventArgs e)
        //{
            //NavigatePage(page);
            //if (File.Exists(Globals.AuthPath))
            //{
            //    try
            //    {
            //        var encryption = Globals.container.GetInstance<IEncryption>();
            //        var auth = JsonConvert.DeserializeObject<AuthFile>(encryption.Decrypt(File.ReadAllText(Globals.AuthPath)));
            //        page.UsernameBox.Text = auth.Username;
            //        page.PasswordBox.Password = auth.Password;
            //        if (auth.SessionId != default)
            //        {
            //            await ProcessLoginAsync(true, auth.SessionId);
            //        }
            //    }
            //    catch (CorruptedAuthSettingsException ex)
            //    {
            //        File.Delete(Globals.AuthPath);
            //        MessageBox.Show(ex.Message, "LightVPN", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return;
            //    }
            //}
        //}
    }
}