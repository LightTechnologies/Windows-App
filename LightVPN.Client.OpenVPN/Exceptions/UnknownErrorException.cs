#nullable enable
namespace LightVPN.Client.OpenVPN.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>
    ///     Thrown when OpenVPN returns a 'Unknown error' output
    /// </summary>
    internal sealed class UnknownErrorException : Exception
    {
        internal UnknownErrorException(string message) : base(message)
        {
        }
    }
}
