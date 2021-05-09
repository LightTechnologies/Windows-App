/* --------------------------------------------
 *
 * OpenVPN TAP Manager - Interface
 * Copyright (C) Light Technologies LLC
 *
 * File: ITapManager.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using System;
using System.Threading.Tasks;

namespace LightVPN.OpenVPN.Interfaces
{
    public interface ITapManager : IDisposable
    {
        bool CheckDriverExists();

        void CreateTapAdapter(string name = "LightVPN-TAP");

        Task InstallDriverAsync();

        bool IsAdapterExistant(string name = "LightVPN-TAP");

        void RemoveTapAdapter(string name = "LightVPN-TAP");
    }
}