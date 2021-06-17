namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <inheritdoc />
    public class ConnectedEventArgs : BaseEventArgs
    {
        public ConnectedEventArgs(string output) : base(output)
        {
        }
    }
}