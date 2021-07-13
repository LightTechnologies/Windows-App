using System;
using DiscordRPC;

namespace LightVPN.Client.Discord.Interfaces
{
    public interface IDiscordRp : IDisposable
    {
        RichPresence Presence { get; set; }

        /// <summary>
        ///     Generates a base presence object
        /// </summary>
        /// <returns>The newly generated presence object</returns>
        RichPresence GetRichPresence();

        /// <summary>
        ///     Resets the presence to the generated base presence and invokes the DiscordRpc client
        /// </summary>
        void ResetPresence();

        /// <summary>
        ///     Disposes the DiscordRpc class
        /// </summary>
        new void Dispose();

        /// <summary>
        ///     Clears the presence completely
        /// </summary>
        void ClearPresence();

        /// <summary>
        ///     Deinitializes the DiscordRpc client
        /// </summary>
        void Deinitialise();

        /// <summary>
        ///     Initializes the DiscordRpc client and sets the presence
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Resets the presence timestamps to null, meaning they disappear
        /// </summary>
        void ResetTimestamps();

        /// <summary>
        ///     Updates the DiscordRpc details
        /// </summary>
        /// <param name="details">The new details you wish to put on the presence</param>
        void UpdateDetails(string details);

        /// <summary>
        ///     Updates the DiscordRpc state
        /// </summary>
        /// <param name="state">The new state you wish to put on the presence</param>
        void UpdateState(string state);

        /// <summary>
        ///     Updates the presence timestamps to the latest timestamps from the current date and time
        /// </summary>
        void UpdateTimestamps();

        /// <summary>
        ///     Sets the presence on the client
        /// </summary>
        void SetPresence();
    }
}
