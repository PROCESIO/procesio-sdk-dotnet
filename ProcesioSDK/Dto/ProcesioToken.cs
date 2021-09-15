﻿using Newtonsoft.Json;

namespace ProcesioSDK.Dto
{
    public class ProcesioToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }
        
        public ProcesioToken() { }
    }
}
