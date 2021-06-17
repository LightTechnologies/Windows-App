namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <summary>
    ///     Event args for the OnConnected event
    /// </summary>
    public sealed class ConnectedEventArgs : BaseEventArgs
    {
        internal ConnectedEventArgs(string output) : base(output)
        {
        }
    }
}