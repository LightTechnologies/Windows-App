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

        public DiscordRpc(DiscordRpcClient client)
        {
            _client = client;
            _client.OnError += Rpc_OnError;
            _client.OnReady += Rpc_Ready;
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
                    LargeImageText = $"Beta [version {Assembly.GetEntryAssembly().GetName().Version}]",
                },
            };
        }

        /// <summary>
        /// Resets the presence to the generated base presence and invokes the DiscordRpc client
        /// </summary>
        public void ClearPresence()
        {
            _client.SetPresence(GetBaseRichPresence());
            _client.Invoke();
        }

        /// <summary>
        /// Deinitializes the DiscordRpc client
        /// </summary>
        public void Deinitialize()
        {
            ClearPresence();
            _client.Deinitialize();
            _client.Invoke();
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
        /// Initializes the DiscordRpc client and invokes it
        /// </summary>
        public void Initialize()
        {
            _client.Initialize();
            _client.Invoke();
        }

        /// <summary>
        /// Resets the presence timestamps to null, meaning they disappear
        /// </summary>
        public void ResetTimestamps()
        {
            _client.UpdateClearTime();
            _client.Invoke();
        }

        /// <summary>
        /// Updates the DiscordRpc details
        /// </summary>
        /// <param name="details">The new details you wish to put on the presence</param>
        public void UpdateDetails(string details)
        {
            _client.UpdateDetails(details);
            _client.Invoke();
        }

        /// <summary>
        /// Updates the DiscordRpc state
        /// </summary>
        /// <param name="state">The new state you wish to put on the presence</param>
        public void UpdateState(string state)
        {
            _client.UpdateState(state);
            _client.Invoke();
        }

        /// <summary>
        /// Updates the presence timestamps to the latest timestamps from the current date and time
        /// </summary>
        public void UpdateTimestamps()
        {
            _client.UpdateStartTime(DateTime.UtcNow);
            _client.Invoke();
        }

        private void Rpc_OnError(object sender, ErrorMessage args)
        {
            throw new RpcException($"[{args.Type}] {args.Message} ({args.Code})");
        }

        private void Rpc_Ready(object sender, ReadyMessage args)
        {
            Debug.WriteLine($"DiscordRpc ready for {args.User.Username}#{args.User.Discriminator}");
        }
    }
}