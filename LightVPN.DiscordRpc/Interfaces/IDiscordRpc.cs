namespace LightVPN.Discord.Interfaces
{
    public interface IDiscordRpc
    {
        void ClearPresence();
        void Deinitialize();
        void Dispose();
        void Initalize();
        void ResetTimestamps();
        void UpdateDetails(string details);
        void UpdateState(string state);
        void UpdateTimestamps();
    }
}