namespace LightVPN.Client.Auth.Exceptions
{
    using System;
    using System.Net;

    /// <inheritdoc />
    /// <summary>
    ///     Thrown when the API returns a undesired response
    /// </summary>
    public sealed class InvalidResponseException : Exception
    {
        internal InvalidResponseException(string message) : base(message)
        {
        }

        internal InvalidResponseException(string message, string responseString, HttpStatusCode code) : base(message)
        {
            this.ResponseString = responseString;
            this.StatusCode = code;
        }

        public string ResponseString { get; }

        public HttpStatusCode StatusCode { get; }
    }
}
