using System;

namespace LightVPN.Discord.Interfaces
{
    public interface IDiscordRpc : IDisposable
    {
        void ClearPresence();

        void Deinitialize();

        new void Dispose();

        void Initialize();

        void ResetTimestamps();

        void UpdateDetails(string details);

        void UpdateState(string state);

        void UpdateTimestamps();
    }
}