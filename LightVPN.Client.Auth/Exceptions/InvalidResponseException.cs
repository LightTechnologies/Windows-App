using System;
using System.Net;

namespace LightVPN.Client.Auth.Exceptions
{
    internal sealed class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        internal InvalidResponseException(string message) : base(message)
        {
        }

        public InvalidResponseException(string message, string responseString, HttpStatusCode code)
        {
            Code = code;
            ResponseString = responseString;
        }

        public HttpStatusCode Code { get; }
        public string ResponseString { get; }
    }
}