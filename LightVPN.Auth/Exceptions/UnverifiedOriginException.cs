using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the LightVPN origin was unverified, and it should not continue being used
    /// </summary>
    public class UnverifiedOriginException : Exception
    {
        public UnverifiedOriginException()
        {
        }

        public UnverifiedOriginException(string message)
            : base(message)
        {
        }

        public UnverifiedOriginException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}