/* --------------------------------------------
 * 
 * Discord Rich Presence - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: DiscordRpc.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * NOTES: This really needs improvements, will get to it soon
 * - Khrysus
 * 
 * --------------------------------------------
 */
using LightVPN.Discord.Interfaces;
using DiscordRPC;
using System;
using System.Threading.Tasks;

namespace LightVPN.Discord
{
    public class DiscordRpc : IDiscordRpc
    {
        public bool _isRunning = false;

        private readonly Assets _assets = null;

        private readonly DiscordRpcClient _client = null;

        private RichPresence _presence = null;

        private Timestamps _timestamps = null;

        private readonly Button[] _buttons = null;
        /// <summary>
        /// Constructs the DiscordRpc wrapper class
        /// </summary>
        /// <param name="client">The DiscordRpcClient instance</param>
        public DiscordRpc(DiscordRpcClient client)
        {
            _client = client;
            _assets = new Assets
            {
                LargeImageKey = "main"
            };
            _buttons = new Button[]
            {
                new Button() { Label = "Visit website", Url = "https://lightvpn.org" },
            };
            _presence = new RichPresence()
            {
                Timestamps = _timestamps,
                Assets = _assets,
                Buttons = _buttons
            };
        }
        /// <summary>
        /// Invokes the DiscordRpc instance
        /// </summary>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> InvokeAsync()
        {
            if (_client == null)
            {
                return Tuple.Create(false, "Client is null or not set to instance of an object");
            }
            await Task.Run(() =>
            {
                _client.Invoke();
            });
            return Tuple.Create(true, string.Empty);
        }
        /// <summary>
        /// Sets the default presence and invokes the DiscordRpc instance
        /// </summary>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> SetPresenceAsync()
        {
            if (_client == null)
            {
                return Tuple.Create(false, "Client is null or not set to instance of an object");
            }
            await Task.Run(() =>
            {
                _presence = new RichPresence
                {
                    Timestamps = _timestamps,
                    Assets = _assets,
                    Buttons = _buttons
                };
                _client.SetPresence(_presence);
                _client.Invoke();
            });
            return Tuple.Create(true, string.Empty);
        }
        /// <summary>
        /// Sets a passed in RichPresence object to be used by the DiscordRpc instance
        /// </summary>
        /// <param name="richPresence"></param>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> SetPresenceObjectAsync(RichPresence richPresence)
        {
            if (_client == null)
            {
                return Tuple.Create(false, "Client is null or not set to instance of an object");
            }
            if (richPresence.State.Contains("Connected"))
                _timestamps = new Timestamps(DateTime.UtcNow);
            else
                _timestamps = null;
            await Task.Run(() =>
            {
                richPresence.Buttons = _buttons;
                richPresence.Assets = _assets;
                richPresence.Timestamps = _timestamps;
                _presence = richPresence;
                _client.SetPresence(_presence);
            });

            return Tuple.Create(true, string.Empty);
        }
        /// <summary>
        /// Starts the DiscordRpc instance
        /// </summary>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> StartAsync()
        {
            if (!_isRunning)
            {
                if (_client == null)
                {
                    return Tuple.Create(false, "Client is null or not set to instance of an object");
                }
                await Task.Run(() =>
                {
                    _client.SetPresence(_presence);
                    _client.Initialize();
                    _client.Invoke();
                });
                _isRunning = true;
                return Tuple.Create(true, string.Empty);
            }
            return Tuple.Create(false, "Client is already running");
        }
        /// <summary>
        /// Stops the DiscordRpc instance
        /// </summary>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> StopAsync()
        {
            if (_isRunning)
            {
                if (_client == null)
                {
                    return Tuple.Create(false, "Client is null or not set to instance of an object");
                }
                await Task.Run(() =>
                {
                    _client.ClearPresence();
                    _client.Deinitialize();
                });
                _isRunning = false;
                return Tuple.Create(true, string.Empty);
            }
            return Tuple.Create(false, "Client is not running");
        }
        /// <summary>
        /// Updates the DiscordRpc instance with fresh timestamps
        /// </summary>
        /// <returns>Tuple of whether it was successful, and if not a description on the error</returns>
        public async Task<Tuple<bool, string>> UpdateTimestampsAsync()
        {
            if (_client == null)
            {
                return Tuple.Create(false, "Client is null or not set to instance of an object");
            }
            await Task.Run(() =>
            {
                //_timestamps = new Timestamps(DateTime.UtcNow);
                //_presence.Timestamps = _timestamps;
            });
            return Tuple.Create(true, string.Empty);
        }
    }
}