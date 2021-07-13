namespace LightVPN.Client.Auth.Exceptions
{
    using System;

    internal sealed class AuthDecryptionException : Exception
    {
        internal AuthDecryptionException(string message) : base(message)
        {
        }
    }
}
