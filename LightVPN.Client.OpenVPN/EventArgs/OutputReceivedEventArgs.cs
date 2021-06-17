namespace LightVPN.Client.OpenVPN.EventArgs
{
    public sealed class OutputReceivedEventArgs : BaseEventArgs
    {
        internal OutputReceivedEventArgs(string output) : base(output)
        {
        }
    }
}