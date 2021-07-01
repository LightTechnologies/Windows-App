using System;
using System.Net;

namespace LightVPN.Client.Auth.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Thrown when the API returns a undesired response
    /// </summary>
    public sealed class InvalidResponseException : Exception
    {
        internal InvalidResponseException(string message) : base(message)
        {
        }

        internal InvalidResponseException(string message, string responseString, HttpStatusCode code) : base(message)
        {
            ResponseString = responseString;
            StatusCode = code;
        }

        public string ResponseString { get; }

        public HttpStatusCode StatusCode { get; }
    }
}
