using DiscordRPC;
using DiscordRPC.Message;
using LightVPN.Discord.Exceptions;
using LightVPN.Discord.Interfaces;
using System;
using System.Diagnostics;
using System.Reflection;

namespace LightVPN.Discord
{
    /// <summary>
    /// The new DiscordRpc class, because the previous one had many issues and needed a rewrite anyway
    /// </summary>
    public class DiscordRpc : IDiscordRpc
    {
        private readonly DiscordRpcClient _client;
        private RichPresence _presence;

        /// <summary>
        /// Constructs the DiscordRpc class
        /// </summary>
        /// <param name="client">The client you want to use</param>
        public DiscordRpc(DiscordRpcClient client)
        {
            _client = client;
            _client.OnError += Rpc_OnError;
            _client.OnReady += Rpc_Ready;
            _presence = GetBaseRichPresence();
            _client.SetPresence(GetBaseRichPresence());
        }

        /// <summary>
        /// Generates a base presence object
        /// </summary>
        /// <returns>The newly generated presence object</returns>
        public static RichPresence GetBaseRichPresence()
        {
            return new RichPresence
            {
                State = "Disconnected",
                Buttons = new Button[]
                {
                    new Button
                    {
                        Label = "Visit our website",
                        Url = "https://lightvpn.org"
                    }
                },
                Assets = new Assets
                {
                    LargeImageKey = "lvpn",
                    LargeImageText = $"Stable [version {Assembly.GetEntryAssembly().GetName().Version}]",
                },
            };
        }

        /// <summary>
        /// Resets the presence to the generated base presence and invokes the DiscordRpc client
        /// </summary>
        public void ResetPresence()
        {
            _presence = GetBaseRichPresence();
        }
        /// <summary>
        /// Clears the presence completely
        /// </summary>
        public void ClearPresence()
        {
            _client.ClearPresence();

        }
        /// <summary>
        /// Deinitializes the DiscordRpc client
        /// </summary>
        public void Deinitialize()
        {
            _client.Deinitialize();
        }

        /// <summary>
        /// Disposes the DiscordRpc class
        /// </summary>
        public void Dispose()
        {
            ClearPresence();
            Deinitialize();
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the DiscordRpc client and sets the presence
        /// </summary>
        public void Initialize()
        {
            if(!_client.IsInitialized)
                _client.Initialize();
            SetPresence();
        }

        /// <summary>
        /// Resets the presence timestamps to null, meaning they disappear
        /// </summary>
        public void ResetTimestamps()
        {
            _presence.WithTimestamps(null);
        }

        /// <summary>
        /// Updates the DiscordRpc details
        /// </summary>
        /// <param name="details">The new details you wish to put on the presence</param>
        public void UpdateDetails(string details)
        {
            _presence.WithDetails(details);
        }

        /// <summary>
        /// Updates the DiscordRpc state
        /// </summary>
        /// <param name="state">The new state you wish to put on the presence</param>
        public void UpdateState(string state)
        {
            _presence.WithState(state);
        }

        /// <summary>
        /// Updates the presence timestamps to the latest timestamps from the current date and time
        /// </summary>
        public void UpdateTimestamps()
        {
            _presence.WithTimestamps(new Timestamps(DateTime.UtcNow));
        }
        /// <summary>
        /// Sets the presence on the client
        /// </summary>
        public void SetPresence()
        {
            if (_client.IsInitialized)
            {
                _client.SetPresence(_presence);
            }
        }
        private void Rpc_OnError(object sender, ErrorMessage args) => throw new RpcException($"[{args.Type}] {args.Message} ({args.Code})");

        private void Rpc_Ready(object sender, ReadyMessage args) => Debug.WriteLine($"DiscordRpc ready for {args.User.Username}#{args.User.Discriminator}");
    }
}