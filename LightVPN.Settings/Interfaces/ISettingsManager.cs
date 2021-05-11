/* --------------------------------------------
 * 
 * Settings manager w/ generics - Interface
 * Copyright (C) Light Technologies LLC
 * 
 * File: ISettingsManager.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using System;
using System.Threading.Tasks;

namespace LightVPN.Settings.Interfaces
{
    public interface ISettingsManager<T>
    {
        T Load();
        Task<T> LoadAsync();
        void Save(T type);
        Task SaveAsync(T type);
    }
}