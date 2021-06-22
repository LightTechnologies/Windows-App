using System;

namespace LightVPN.Client.OpenVPN.EventArgs
{
    /// <inheritdoc />
    public class ErrorEventArgs : System.EventArgs
    {
        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; init; }
    }
}