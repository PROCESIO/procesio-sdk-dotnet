using Newtonsoft.Json;

namespace ProcesioSDK.Utilities
{
    public class ProcesioTokens
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }
        
        public ProcesioTokens() { }
        public ProcesioTokens(string accesToken, string refreshToken, int expires_in)
        {
            AccessToken = accesToken;
            RefreshToken = refreshToken;
            Expires_in = expires_in;
        }
    }
}
