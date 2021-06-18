namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <inheritdoc />
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