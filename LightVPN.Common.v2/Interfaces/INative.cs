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
namespace LightVPN.Common.Interfaces
{
    public interface INative
    {
        bool IsRunningOnStartup();

        void EnableRunOnStartup();

        void DisableRunOnStartup();
    }
}