using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when the API responds with something that we can't understand
    /// </summary>
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message)
            : base(message)
        {
        }

        public InvalidResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidResponseException(string message, string response, int code)
            : base(message)
        {
            ResponseString = response;
            ErrorCode = code;
        }

        public int ErrorCode { get; set; }

        public string ResponseString { get; set; }
    }
}