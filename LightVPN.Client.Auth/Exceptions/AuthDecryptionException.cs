using System;

namespace LightVPN.Client.Auth.Exceptions
{
    public class AuthDecryptionException : Exception
    {
        public AuthDecryptionException(string message) : base(message)
        {
        }
    }
}