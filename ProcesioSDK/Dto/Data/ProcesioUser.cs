using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProcesioSDK.Dto.Data
{
    /// <summary>
    /// The class used for the parameters that are given to the Authenticate method
    /// </summary>
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
        public ProcesioUser(string userName, string password, string realm, string clientId, string grantType = "password")
        {
            UserName = userName;
            Password = password;
            Realm = realm;
            ClientId = clientId;
            GrantType = grantType;
        }

        internal Dictionary<string, string> GetAuthenticationInformation()
        {
            var queryString = new Dictionary<string, string>
            {
                { "realm", Realm },
                { "grant_type", GrantType },
                { "username", UserName },
                { "password", Password },
                { "client_id", ClientId }
            };

            return queryString;
        }
    }
}
