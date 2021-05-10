using System;
using System.Net.Http;

namespace LightVPN.Auth
{
    public class SSLCheckingHttpClient : HttpClient
    {
        public SSLCheckingHttpClient(HttpClientHandler handler) : base(handler)
        {
            base.DefaultRequestVersion = new Version("2.0.0.0");
            base.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "LightVPN/1.0");
        }
    }
}