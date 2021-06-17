#nullable enable
using System;

namespace LightVPN.Client.OpenVPN.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    ///     Thrown when OpenVPN returns a 'Unknown error' output
    /// </summary>
    internal sealed class UnknownErrorException : Exception
    {
        public UnknownErrorException(string message) : base(message)
        {
        }
    }
}