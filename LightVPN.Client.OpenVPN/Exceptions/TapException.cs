namespace LightVPN.Client.OpenVPN.Exceptions
{
    using System;

    /// <inheritdoc />
    internal sealed class TapException : Exception
    {
        internal TapException(string message) : base(message)
        {
        }
    }
}
