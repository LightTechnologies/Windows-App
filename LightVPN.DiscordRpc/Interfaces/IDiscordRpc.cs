/* --------------------------------------------
 * 
 * Discord Rich Presence - Interface
 * Copyright (C) Light Technologies LLC
 * 
 * File: IDiscordRpc.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using DiscordRPC;
using System;
using System.Threading.Tasks;

namespace LightVPN.Discord.Interfaces
{
    public interface IDiscordRpc
    {
        Task<Tuple<bool, string>> InvokeAsync();

        Task<Tuple<bool, string>> SetPresenceAsync();

        Task<Tuple<bool, string>> SetPresenceObjectAsync(RichPresence richPresence);

        Task<Tuple<bool, string>> StartAsync();

        Task<Tuple<bool, string>> StopAsync();
    }
}