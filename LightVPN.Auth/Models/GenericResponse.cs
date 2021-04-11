using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.Auth.Models
{
    public class GenericResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}
