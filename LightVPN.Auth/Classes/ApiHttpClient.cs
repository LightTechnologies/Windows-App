using LightVPN.Auth.Exceptions;
using LightVPN.Auth.Models;
using LightVPN.FileLogger;
using LightVPN.FileLogger.Base;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.Auth
{
    public class ApiHttpClient : HttpClient
    {
        private readonly FileLoggerBase _logger = new ErrorLogger();

        public ApiHttpClient(HttpClientHandler handler) : base(handler)
        {
            base.DefaultRequestVersion = new Version("2.0.0.0");
            base.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "LightVPN/1.0");
        }
        public async Task CheckResponseAsync(HttpResponseMessage resp, CancellationToken cancellationToken = default)
        {
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
            }
            if (!resp.IsSuccessStatusCode) throw new InvalidResponseException("The API seems to be down, or sending back invalid responses, please try again later.");
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.Write("(Http/Get) Getting data");
                var resp = await this.GetAsync(url, cancellationToken);
                await CheckResponseAsync(resp, cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(await resp.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw new ApiOfflineException("Failed to connect to the internet, please check your internet connection");
            }
        }

        public async Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.Write("(Http/Post) Posting JSON data");
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