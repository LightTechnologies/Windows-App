using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the users subscription has expired
    /// </summary>
    public class SubscriptionExpiredException : Exception
    {
        public SubscriptionExpiredException()
        {
        }

        public SubscriptionExpiredException(string message)
            : base(message)
        {
        }

        public SubscriptionExpiredException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}