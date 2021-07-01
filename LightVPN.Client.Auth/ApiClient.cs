using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Auth.Resources;
using LightVPN.Client.Auth.Utils;
using LightVPN.Client.Debug;
using LightVPN.Client.Windows.Common;

namespace LightVPN.Client.Auth
{
    /// <inheritdoc cref="System.Net.Http.HttpClient" />
    /// <summary>
    ///     Custom implementation for HttpClient that makes talking to the LightVPN API a hell of a lot easier
    /// </summary>
    public sealed class ApiClient : HttpClient, IApiClient
    {
        /// <summary>
        ///     The authentication data for the current user
        /// </summary>
        private AuthResponse AuthData { get; }

        public ApiClient()
        {
            DebugLogger.Write("lvpn-client-auth-apiclient", $"ctor called");

            DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LightVPN", "3.0"));
            DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Version",
                $"{Environment.OSVersion.Platform.ConvertPlatformToString()} {Assembly.GetEntryAssembly()?.GetName().Version}");
            DefaultRequestVersion = new Version("2.0.0.0");
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            BaseAddress = new Uri("https://lightvpn.org/api/");

            try
            {
                AuthData = AuthDataManager.Read();

                if (!string.IsNullOrWhiteSpace(AuthData.SessionId) && !string.IsNullOrWhiteSpace(AuthData.UserName))
                    AssignSessionHeader(AuthData.UserName, AuthData.SessionId);
            }
            catch (JsonException)
            {
                AuthData = new AuthResponse();
            }
        }

        /// <summary>
        ///     Sends a GET request to the specified URL path
        /// </summary>
        /// <param name="url">URL path</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The object to attempt to parse to</typeparam>
        /// <returns>If successful, the response deserialized to an object</returns>
        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            var resp = await GetAsync(url, cancellationToken);
            return await CheckResponse<T>(resp, cancellationToken);
        }

        /// <summary>
        ///     Sends a POST request to the specified URL path
        /// </summary>
        /// <param name="url">URL path</param>
        /// <param name="body">Body to be serialized to JSON and sent</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The object to attempt to parse to</typeparam>
        /// <returns>If successful, the response deserialized to an object</returns>
        public async Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken = default)
        {
            var resp = await PostAsync(url,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                cancellationToken);
            return await CheckResponse<T>(resp, cancellationToken);
        }

        /// <summary>
        ///     Assigns the Authorization header for API authentication
        /// </summary>
        /// <param name="userName">Username of the user</param>
        /// <param name="sessionId">ID pointing to an active session for the specified user</param>
        private void AssignSessionHeader(string userName, string sessionId)
        {
            DebugLogger.Write("lvpn-client-auth-apiclient", $"assigned sess header");

            DefaultRequestHeaders.Remove("Authorization");
            DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{userName} {sessionId}");
        }

        /// <summary>
        ///     Checks a response message and if successful, returns the response serialized to an object
        /// </summary>
        /// <param name="responseMessage">The response message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The object to check and serialize to</typeparam>
        /// <returns>The response serialized to the object specified in the type parameter</returns>
        /// <exception cref="UpdateRequiredException">Thrown when the API requires the client to be updated</exception>
        /// <exception cref="InvalidResponseException">Thrown when a undesired response is received from the API</exception>
        private async Task<T> CheckResponse<T>(HttpResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
        {
            var responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                try
                {
                    var genericResponse =
                        await JsonSerializer.DeserializeAsync<GenericResponse>(
                            await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                            cancellationToken: cancellationToken);

                    if (responseMessage.StatusCode == HttpStatusCode.UpgradeRequired)
                        throw new UpdateRequiredException("You need to update your client version!");

                    if (!string.IsNullOrWhiteSpace(genericResponse.Message))
                    {
                        throw new InvalidResponseException(genericResponse.Message, responseString, responseMessage.StatusCode);
                    }
                }
                catch (JsonException)
                {
                    // ignored.
                }


            }

            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new InvalidResponseException(StringTable.API_BAD_REQUEST, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.Unauthorized:
                    throw new InvalidResponseException(StringTable.API_UNAUTH, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.Forbidden:
                    throw new InvalidResponseException(StringTable.API_FORBID, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.NotFound:
                    throw new InvalidResponseException(StringTable.API_CLIENT_ISSUE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.MethodNotAllowed:
                    throw new InvalidResponseException(StringTable.API_UNAVAILABLE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.RequestTimeout:
                    throw new InvalidResponseException(StringTable.API_UNAVAILABLE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.PreconditionFailed:
                    throw new InvalidResponseException(StringTable.API_CLIENT_ISSUE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.RequestEntityTooLarge:
                    throw new InvalidResponseException(StringTable.API_CLIENT_ISSUE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.UpgradeRequired:
                    throw new UpdateRequiredException("You need to update your client version!");
                case HttpStatusCode.TooManyRequests:
                    throw new InvalidResponseException(StringTable.API_RATELIMITED, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.InternalServerError:
                    throw new InvalidResponseException(StringTable.API_SERVER_ERROR, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.BadGateway:
                    throw new InvalidResponseException(StringTable.API_UNAVAILABLE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.ServiceUnavailable:
                    throw new InvalidResponseException(StringTable.API_UNAVAILABLE, responseString,
                        responseMessage.StatusCode);
                case HttpStatusCode.GatewayTimeout:
                    throw new InvalidResponseException(StringTable.API_UNAVAILABLE, responseString,
                        responseMessage.StatusCode);
            }

            try
            {
                if (typeof(T) == typeof(byte[]))
                {
                    object respByteArray = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
                    return (T)respByteArray;
                }

                object respJson = await JsonSerializer.DeserializeAsync<T>(
                    await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                if (respJson is not AuthResponse authResponse) return (T)respJson;

                authResponse.UserName = Globals.UserName;

                AssignSessionHeader(Globals.UserName, authResponse.SessionId);
                AuthDataManager.Write(authResponse);

                return (T)respJson;
            }
            catch (JsonException)
            {
                throw new InvalidResponseException(StringTable.API_CLIENT_ISSUE, responseString,
                    responseMessage.StatusCode);
            }
        }
    }
}
