using Newtonsoft.Json;

namespace SDKProcesio.Utilities
{
    public class ProcesioTokens
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }
    }
}
