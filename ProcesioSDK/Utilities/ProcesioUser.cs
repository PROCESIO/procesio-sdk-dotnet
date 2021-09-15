using Newtonsoft.Json;

namespace ProcesioSDK.Utilities
{
    public class ProcesioUser
    {
        [JsonProperty("username")]
        public string UserName{get;set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("realm")]
        public string Realm { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        public ProcesioUser() { }
        public ProcesioUser(string userName, string password, string realm, string clientId, string grantType)
        {
            UserName = userName;
            Password = password;
            Realm = realm;
            ClientId = clientId;
            GrantType = grantType;
        }
    }
}
