using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.Auth.Classes
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
