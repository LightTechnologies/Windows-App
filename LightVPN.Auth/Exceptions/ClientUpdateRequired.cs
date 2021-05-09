using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the client needs an upgrade
    /// </summary>
    public class ClientUpdateRequired : Exception
    {
        public ClientUpdateRequired()
        {
        }

        public ClientUpdateRequired(string message)
            : base(message)
        {
        }

        public ClientUpdateRequired(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}