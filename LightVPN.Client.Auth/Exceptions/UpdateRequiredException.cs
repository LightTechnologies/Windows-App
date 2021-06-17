using System;

namespace LightVPN.Client.Auth.Exceptions
{
    internal sealed class UpdateRequiredException : Exception
    {
        public UpdateRequiredException()
        {
        }

        internal UpdateRequiredException(string message) : base(message)
        {
        }
    }
}