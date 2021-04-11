/* --------------------------------------------
 * 
 * API exceptions - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: ApiException.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using System;

namespace LightVPN.Auth
{
    public class ApiException
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

        public class ApiOfflineException : Exception
        {
            public ApiOfflineException()
            {
            }

            public ApiOfflineException(string message)
                : base(message)
            {
            }

            public ApiOfflineException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class RatelimitedException : Exception
        {
            public RatelimitedException()
            {
            }

            public RatelimitedException(string message)
                : base(message)
            {
            }

            public RatelimitedException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        /// <summary>
        /// Thrown when the LightVPN origin was unverified, and it should not continue being used
        /// </summary>
        public class UnverifiedOriginException : Exception
        {
            public UnverifiedOriginException()
            {
            }

            public UnverifiedOriginException(string message)
                : base(message)
            {
            }

            public UnverifiedOriginException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        /// <summary>
        /// Thrown when the users subscription has expired
        /// </summary>
        public class SubscriptionExpiredException : Exception
        {
            public SubscriptionExpiredException()
            {
            }

            public SubscriptionExpiredException(string message)
                : base(message)
            {
            }

            public SubscriptionExpiredException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
        /// <summary>
        /// Thrown when the client needs an upgrade
        /// </summary>
        public class ClientUpdateRequired : Exception
        {
            public ClientUpdateRequired()
            {
            }

            public ClientUpdateRequired(string message)
                : base(message)
            {
            }

            public ClientUpdateRequired(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
        /// <summary>
        /// Thrown when the API responds with something that we can't understand
        /// </summary>
        public class InvalidResponseException : Exception
        {
            public string ResponseString { get; set; }
            public int ErrorCode { get; set; }
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
        }
    }
}
