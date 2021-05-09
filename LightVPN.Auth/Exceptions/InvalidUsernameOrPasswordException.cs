using System;

namespace LightVPN.Auth.Exceptions
{
    /// <summary>
    /// Thrown when a invalid username or password was returned by the API
    /// </summary>
    public class InvalidUsernameOrPasswordException : Exception
    {
        public InvalidUsernameOrPasswordException()
        {
        }

        public InvalidUsernameOrPasswordException(string message)
            : base(message)
        {
        }

        public InvalidUsernameOrPasswordException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}