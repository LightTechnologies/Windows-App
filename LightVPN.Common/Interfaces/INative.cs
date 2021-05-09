/* --------------------------------------------
 *
 * Native methods - Interface
 * Copyright (C) Light Technologies LLC
 *
 * File: INative.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using System;

namespace LightVPN.Common.Interfaces
{
    public interface INative : IDisposable
    {
        void DisableRunOnStartup();

        void Dispose();

        void EnableRunOnStartup();

        bool IsRunningOnStartup();
    }
}