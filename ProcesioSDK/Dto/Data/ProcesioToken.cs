﻿using Newtonsoft.Json;
using System.Net;

namespace ProcesioSDK.Dto.Data
{
    /// <summary>
    /// The class used for the response of Authentication method
    /// </summary>
    public class ProcesioToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// If status code is not 200OK then Authentication failed
        /// </summary>
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }
        
        public ProcesioToken() { }
    }
}
