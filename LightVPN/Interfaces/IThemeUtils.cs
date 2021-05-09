/* --------------------------------------------
 *
 * Theme utilities - Interface
 * Copyright (C) Light Technologies LLC
 *
 * File: ThemeUtils.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using LightVPN.Auth.Models;

namespace LightVPN.Interfaces
{
    public interface IThemeUtils
    {
        void SwitchTheme(Theme theme);
    }
}