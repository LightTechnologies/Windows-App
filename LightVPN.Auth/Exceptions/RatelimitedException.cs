using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the user has been ratelimited by Cloudflare
    /// </summary>
    public class RatelimitedException : Exception
    {
        public RatelimitedException()
        {
        }

        public RatelimitedException(string message)
            : base(message)
        {
        }

        public RatelimitedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}