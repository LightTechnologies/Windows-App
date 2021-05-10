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
using System.Threading.Tasks;

namespace LightVPN.OpenVPN.Interfaces
{
    public interface IManager : IDisposable
    {
        event Manager.ConnectedEvent Connected;

        event Manager.ErrorEvent Error;

        event Manager.OutputReceived OnOutput;

        event Manager.OutputEvent Output;

        bool IsConnected { get; }

        bool IsDisposed { get; }

        void Connect(string configpath);

        void Disconnect();

        Task PerformAutoTroubleshootAsync(bool isServerRelated, string invokationMessage);
    }
}