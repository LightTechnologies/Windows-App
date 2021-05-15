using LightVPN.Auth.Exceptions;
using LightVPN.Auth.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Auth
{
    /// <summary>
    /// Custom class derived from HttpClient that handles all the API authorization for us
    /// </summary>
    public class ApiHttpClient : HttpClient
    {
        /// <summary>
        /// Constructs the class
        /// </summary>
        /// <param name="handler">Any spare arguments you want the client to take into account</param>
        public ApiHttpClient(HttpClientHandler handler) : base(handler)
        {
            base.DefaultRequestVersion = new Version("2.0.0.0");
            base.BaseAddress = new Uri("https://lightvpn.org/api/");
            base.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "LightVPN/1.0");
        }
        /// <summary>
        /// Validates a response and throws the appropriate exception if something is not right
        /// </summary>
        /// <param name="resp">The response message</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidResponseException"></exception>
        /// <exception cref="RatelimitedException"></exception>
        /// <exception cref="ClientUpdateRequired"></exception>
        /// <exception cref="ApiOfflineException"></exception>
        /// <returns></returns>
        public async Task CheckResponseAsync(HttpResponseMessage resp, CancellationToken cancellationToken = default)
        {
            var content = await resp.Content.ReadAsStreamAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                try
                {
                    var errorresp = await JsonSerializer.DeserializeAsync<GenericResponse>(content, cancellationToken: cancellationToken);

                    throw new InvalidResponseException(errorresp.Message);
                }
                catch (JsonException)
                {
                }
            }

            switch (resp.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new InvalidResponseException("Bad request, a manual client update may be required. Please visit the LightVPN dashboard and download the latest installer.");
                case HttpStatusCode.NotFound:
                    throw new InvalidResponseException("Not found, a manual client update may be required. Please visit the LightVPN dashboard and download the latest installer.");
                case HttpStatusCode.Unauthorized:
                    throw new InvalidResponseException("You've been blocked by the edge firewall. You could've requested from a blocked location or typed something in that could cause issues.");
                case HttpStatusCode.InternalServerError:
                    throw new InvalidResponseException("A server error has occurred. Please contact support to inform us about this issue.");
                case HttpStatusCode.TooManyRequests:
                    throw new RatelimitedException("You've been ratelimited due to you sending too many requests, try again in a minute.");
                case HttpStatusCode.UpgradeRequired:
                    throw new ClientUpdateRequired("An update is available.");
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.NoContent:
                    break;
                default:
                    throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.");
            }
        }
        /// <summary>
        /// Gets the specified page and parses it to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The object to parse the response to</typeparam>
        /// <param name="url">The URL of the API you want to request</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidResponseException"></exception>
        /// <exception cref="RatelimitedException"></exception>
        /// <exception cref="ClientUpdateRequired"></exception>
        /// <exception cref="ApiOfflineException"></exception>
        /// <returns>The serialized object</returns>
        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var resp = await this.GetAsync(url, cancellationToken);
                await CheckResponseAsync(resp, cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(await resp.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw new ApiOfflineException("Failed to connect to the internet, please check your internet connection");
            }
        }
        /// <summary>
        /// Posts JSON data to the specified page and parses the response to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The object you want to parse the response to</typeparam>
        /// <param name="url">The URL of the API you want to request</param>
        /// <param name="body">The body which will be serialized to JSON</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidResponseException"></exception>
        /// <exception cref="RatelimitedException"></exception>
        /// <exception cref="ClientUpdateRequired"></exception>
        /// <exception cref="ApiOfflineException"></exception>
        /// <returns>The serialized object</returns>
        public async Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var resp = await this.PostAsync(url, content, cancellationToken);
                await CheckResponseAsync(resp, cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(await resp.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw new ApiOfflineException("Failed to connect to the internet, please check your internet connection");
            }
        }
    }
}