namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <inheritdoc />
    /// <summary>
    ///     Base class for all event args in the OpenVPN namespace
    /// </summary>
    public abstract class BaseEventArgs : System.EventArgs
    {
        protected BaseEventArgs(string output)
        {
            Output = output;
        }

        public string Output { get; set; }
    }
}