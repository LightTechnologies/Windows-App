using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Auth.Exceptions;
using LightVPN.Client.Auth.Interfaces;
using LightVPN.Client.Auth.Models;
using LightVPN.Client.Auth.Resources;
using LightVPN.Client.Auth.Utils;

namespace LightVPN.Client.Auth
{
    public class ApiClient : HttpClient, IApiClient
    {
        public AuthResponse AuthData { get; }

        public ApiClient()
        {
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

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            var resp = await GetAsync(url, cancellationToken);
            return await CheckResponse<T>(resp, cancellationToken);
        }

        public async Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken = default)
        {
            var resp = await PostAsync(url, new StringContent(JsonSerializer.Serialize(body)), cancellationToken);
            return await CheckResponse<T>(resp, cancellationToken);
        }

        private void AssignSessionHeader(string userName, string sessionId)
        {
            DefaultRequestHeaders.Remove("Authorization");
            DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{userName} {sessionId}");
        }

        private async Task<T> CheckResponse<T>(HttpResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                var genericResponse =
                    await JsonSerializer.DeserializeAsync<GenericResponse>(
                        await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken);

                if (responseMessage.StatusCode == HttpStatusCode.UpgradeRequired)
                    throw new UpdateRequiredException("You need to update your client version!");

                throw new InvalidResponseException($"{genericResponse?.Message} ({genericResponse?.Code})");
            }

            var responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

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
                object respJson = await JsonSerializer.DeserializeAsync<T>(
                    await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                if (respJson is not AuthResponse authResponse) return (T)respJson;

                AssignSessionHeader(authResponse.UserName, authResponse.SessionId);
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