namespace LightVPN.Client.OpenVPN.EventArgs
{
    using System;

    /// <inheritdoc />
    public sealed class ErrorEventArgs : EventArgs
    {
        internal ErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
