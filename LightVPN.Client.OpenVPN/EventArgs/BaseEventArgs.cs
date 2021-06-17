namespace LightVPN.Client.OpenVPN.EventArgs
{
    public abstract class BaseEventArgs : System.EventArgs
    {
        protected BaseEventArgs(string output)
        {
            Output = output;
        }

        internal string Output { get; set; }
    }
}