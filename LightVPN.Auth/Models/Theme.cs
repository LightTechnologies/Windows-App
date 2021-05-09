/* --------------------------------------------
 *
 * Theme model - Model
 * Copyright (C) Light Technologies LLC
 *
 * File: Theme.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

namespace LightVPN.Auth.Models
{
    public struct Theme
    {
        public bool DarkMode { set; get; }

        public string PrimaryColor { set; get; }

        public string SecondaryColor { set; get; }
    }
}