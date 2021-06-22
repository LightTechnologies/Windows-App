using System;

namespace LightVPN.Client.Auth.Exceptions
{
    internal sealed class AuthDecryptionException : Exception
    {
        internal AuthDecryptionException(string message) : base(message)
        {
        }
    }
}