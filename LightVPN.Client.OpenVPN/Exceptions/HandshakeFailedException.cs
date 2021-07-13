namespace LightVPN.Client.OpenVPN.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>
    ///     Thrown when the TLS handshake fails with OpenVPN
    /// </summary>
    internal sealed class HandshakeFailedException : Exception
    {
        internal HandshakeFailedException(string message) : base(message)
        {
        }
    }
}
