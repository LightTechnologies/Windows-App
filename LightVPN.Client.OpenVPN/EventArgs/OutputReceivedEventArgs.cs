namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <inheritdoc />
    /// <summary>
    ///     Event arguments for the OnOutputReceived event
    /// </summary>
    public sealed class OutputReceivedEventArgs : BaseEventArgs
    {
        internal OutputReceivedEventArgs(string output) : base(output)
        {
        }
    }
}