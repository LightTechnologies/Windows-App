#nullable enable
using System;

namespace LightVPN.Client.OpenVPN.Exceptions
{
    /// <inheritdoc />
    public class UnknownErrorException : Exception
    {
        public UnknownErrorException()
        {
        }

        public UnknownErrorException(string message) : base(message)
        {
        }

        public UnknownErrorException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}