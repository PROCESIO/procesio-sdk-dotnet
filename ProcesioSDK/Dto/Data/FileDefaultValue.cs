using Newtonsoft.Json;

namespace ProcesioSDK.Dto.Data
{
    public class FileDefaultValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
