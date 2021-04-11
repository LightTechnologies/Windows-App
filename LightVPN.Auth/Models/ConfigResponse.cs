using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.Auth.Models
{
    public class ConfigResponse : GenericResponse
    {
        [JsonProperty("bytes")]
        public string ConfigArchiveBase64 { get; set; }
    }
}
