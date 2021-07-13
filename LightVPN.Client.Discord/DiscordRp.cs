namespace LightVPN.Client.Discord
{
    using System;
    using DiscordRPC;
    using DiscordRPC.Logging;
    using Interfaces;
    using Models;

    public sealed class DiscordRp : IDiscordRp
    {
        private readonly DiscordRpConfiguration _configuration;
        private DiscordRpcClient _client;

        public RichPresence Presence { get; set; }

        public DiscordRp(DiscordRpConfiguration configuration)
        {
            this._configuration = configuration;

            this.CreateInstance();
        }

        private void CreateInstance()
        {
            this._client = new DiscordRpcClient(this._configuration.ClientId.ToString(),
                logger: new FileLogger("discord.log"));

            this.Presence = this.GetRichPresence();
            this._client.SetPresence(this.Presence);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates a base presence object
        /// </summary>
        /// <returns>The newly generated presence object</returns>
        public RichPresence GetRichPresence()
        {
            return new()
            {
                State = "Disconnected",
                Buttons = new[]
                {
                    new Button
                    {
                        Label = "Visit us",
                        Url = "https://lightvpn.org",
                    },
                },
                Assets = new Assets
                {
                    LargeImageKey = this._configuration.LargeImageKey,
                    LargeImageText = this._configuration.LargeImageText,
                },
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resets the presence to the generated base presence and invokes the DiscordRpc client
        /// </summary>
        public void ResetPresence()
        {
            this.Presence = this.GetRichPresence();
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        /// <summary>
        ///     Disposes the DiscordRpc class
        /// </summary>
        public void Dispose()
        {
            this.ClearPresence();
            this.Deinitialise();
            this._client.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Clears the presence completely
        /// </summary>
        public void ClearPresence()
        {
            this._client.ClearPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     De-initialises the DiscordRpc client
        /// </summary>
        public void Deinitialise()
        {
            if (!this._client.IsInitialized) return;

            this._client.Deinitialize();
            this._client.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes the DiscordRpc client and sets the presence
        /// </summary>
        public void Initialize()
        {
            if (!this._client.IsInitialized || this._client.IsDisposed)
            {
                this.CreateInstance();
                this._client.Initialize();
            }

            this.SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resets the presence timestamps to null, meaning they disappear
        /// </summary>
        public void ResetTimestamps()
        {
            this.Presence.WithTimestamps(null);

            this.SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the DiscordRpc details
        /// </summary>
        /// <param name="details">The new details you wish to put on the presence</param>
        public void UpdateDetails(string details)
        {
            this.Presence.WithDetails(details);

            this.SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the DiscordRpc state
        /// </summary>
        /// <param name="state">The new state you wish to put on the presence</param>
        public void UpdateState(string state)
        {
            this.Presence.WithState(state);

            this.SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the presence timestamps to the latest timestamps from the current date and time
        /// </summary>
        public void UpdateTimestamps()
        {
            this.Presence.WithTimestamps(new Timestamps(DateTime.UtcNow));

            this.SetPresence();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Sets the presence on the client
        /// </summary>
        public void SetPresence()
        {
            if (!this._client.IsInitialized) return;

            this._client.SetPresence(this.Presence);
            this._client.Invoke();
        }
    }
}
