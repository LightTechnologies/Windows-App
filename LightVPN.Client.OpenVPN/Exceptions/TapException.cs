using System;

namespace LightVPN.Client.OpenVPN.Exceptions
{
    /// <inheritdoc />
    internal sealed class TapException : Exception
    {
        internal TapException(string message) : base(message)
        {

        }
    }
}
