﻿using Newtonsoft.Json;

namespace LightVPN.Auth.Models
{
    public class GenericResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }
    }
}