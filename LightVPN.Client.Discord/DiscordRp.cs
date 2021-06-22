using System;
using DiscordRPC;
using DiscordRPC.Logging;
using LightVPN.Client.Discord.Interfaces;
using LightVPN.Client.Discord.Models;

namespace LightVPN.Client.Discord
{
    public sealed class DiscordRp : IDiscordRp
    {
        private readonly DiscordRpConfiguration _configuration;
        private DiscordRpcClient _client;

        public RichPresence Presence { get; set; }

        public DiscordRp(DiscordRpConfiguration configuration)
        {
            _configuration = configuration;

            CreateInstance();
        }

        private void CreateInstance()
        {
            _client = new DiscordRpcClient(_configuration.ClientId.ToString(), logger: new FileLogger("discord.log"));

            Presence = GetRichPresence();
            _client.SetPresence(Presence);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates a base presence object
        /// </summary>
        /// <returns>The newly generated presence object</returns>
        public RichPresence GetRichPresence()
        {
            return new RichPresence
            {
                State = "Disconnected",
                Buttons = new[]
                {
                    new Button
                    {
                        Label = "Visit us",
                        Url = "https://lightvpn.org"
                    }
                },
                Assets = new Assets
                {
                    LargeImageKey = _configuration.LargeImageKey,
                    LargeImageText = _configuration.LargeImageText
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resets the presence to the generated base presence and invokes the DiscordRpc client
        /// </summary>
        public void ResetPresence()
        {
            Presence = GetRichPresence();
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        /// <summary>
        ///     Disposes the DiscordRpc class
        /// </summary>
        public void Dispose()
        {
            ClearPresence();
            Deinitialize();
            _client.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Clears the presence completely
        /// </summary>
        public void ClearPresence()
        {
            _client.ClearPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Deinitializes the DiscordRpc client
        /// </summary>
        public void Deinitialize()
        {
            if (!_client.IsInitialized) return;

            _client.Deinitialize();
            _client.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes the DiscordRpc client and sets the presence
        /// </summary>
        public void Initialize()
        {
            if (!_client.IsInitialized || _client.IsDisposed)
            {
                CreateInstance();
                _client.Initialize();
            }

            SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resets the presence timestamps to null, meaning they disappear
        /// </summary>
        public void ResetTimestamps()
        {
            Presence.WithTimestamps(null);

            SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the DiscordRpc details
        /// </summary>
        /// <param name="details">The new details you wish to put on the presence</param>
        public void UpdateDetails(string details)
        {
            Presence.WithDetails(details);

            SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the DiscordRpc state
        /// </summary>
        /// <param name="state">The new state you wish to put on the presence</param>
        public void UpdateState(string state)
        {
            Presence.WithState(state);

            SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the presence timestamps to the latest timestamps from the current date and time
        /// </summary>
        public void UpdateTimestamps()
        {
            Presence.WithTimestamps(new Timestamps(DateTime.UtcNow));

            SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Sets the presence on the client
        /// </summary>
        public void SetPresence()
        {
            if (!_client.IsInitialized) return;

            _client.SetPresence(Presence);
            _client.Invoke();
        }
    }
}