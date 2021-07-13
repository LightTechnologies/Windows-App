namespace LightVPN.Client.Auth.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>
    ///     Thrown when the API requires the client to be more up-to-date
    /// </summary>
    public sealed class UpdateRequiredException : Exception
    {
        internal UpdateRequiredException(string message) : base(message)
        {
        }
    }
}
