/* --------------------------------------------
 * 
 * Login view - Model
 * Copyright (C) Light Technologies LLC
 * 
 * File: Login.xaml.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Auth.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LightVPN.Common.Models;
using LightVPN.Settings.Interfaces;
using System.Diagnostics;

namespace LightVPN.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        /*
            I can't be bothered to MVVM the Login window I'll do it later.
        */
        public static readonly DependencyProperty IsAuthenticatingProperty =
            DependencyProperty.Register("IsAuthenticating", typeof(bool),
            typeof(Page), new(false));
        public bool IsAuthenticating
        {
            get { return (bool)GetValue(IsAuthenticatingProperty); }
            set { SetValue(IsAuthenticatingProperty, value); }
        }
        public Login()
        {
            InitializeComponent();
        }
    }
}
