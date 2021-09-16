using Newtonsoft.Json;
using ProcesioSDK.Config;
using System.Collections.Generic;

namespace ProcesioSDK.Dto.Data
{
    /// <summary>
    /// The class used for the parameters that are given to the Authenticate method
    /// </summary>
    public class ProcesioUser
    {
        [JsonProperty("username")]
        public string UserName { get; protected set; }

        [JsonProperty("password")]
        public string Password { get; protected set; }

        [JsonProperty("grant_type")]
        public string GrantType { get; protected set; }

        public ProcesioUser() { }
        public ProcesioUser(string userName, string password, string grantType = "password")
        {
            UserName = userName;
            Password = password;
            GrantType = grantType;
        }

        internal Dictionary<string, string> GetAuthenticationInformation(ProcesioConfig config)
        {
            var queryString = new Dictionary<string, string>
            {
                { "realm", config.AuthenticationRealm },
                { "client_id", config.AuthenticationClientId },
                { "username", UserName },
                { "password", Password },
                { "grant_type", GrantType }
            };

            return queryString;
        }
    }
}
