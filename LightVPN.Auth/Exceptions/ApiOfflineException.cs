using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the API is offline
    /// </summary>
    public class ApiOfflineException : Exception
    {
        public ApiOfflineException()
        {
        }

        public ApiOfflineException(string message)
            : base(message)
        {
        }

        public ApiOfflineException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}