/* --------------------------------------------
 * 
 * OpenVPN Manager - Interface
 * Copyright (C) Light Technologies LLC
 * 
 * File: IManager.cs
 * 
 * Created: 04-03-21 Toshiro
 * 
 * --------------------------------------------
 */

using System;

namespace LightVPN.OpenVPN.Interfaces
{
    public interface IManager : IDisposable
    {
        bool IsConnected { get; }
        bool IsDisposed { get; }

        event Manager.ConnectedEvent Connected;
        event Manager.LoginFailedEvent LoginFailed;
        event Manager.outputRecieved OnOutput;
        event Manager.OutputEvent Output;
        event Manager.ErrorEvent Error;

        void Connect(string configpath);
        void Disconnect();
    }
}