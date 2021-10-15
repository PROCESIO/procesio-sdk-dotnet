using Newtonsoft.Json;

namespace ProcesioSDK.Dto.Data
{
    public class ProcesioApiKey
    {
        [JsonProperty("api_key")]
        public string ApiKeyName { get; set; }

        [JsonProperty("api_value")]
        public string ApiKeyValue { get; set; }

        public ProcesioApiKey(string apiKeyName, string apiKeyValue)
        {
            ApiKeyName = apiKeyName;
            ApiKeyValue = apiKeyValue;
        }
    }
}
