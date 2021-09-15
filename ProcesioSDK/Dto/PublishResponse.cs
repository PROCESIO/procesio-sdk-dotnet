using Newtonsoft.Json;

namespace ProcesioSDK.Dto
{
    public class PublishResponse
    {
        [JsonProperty("flows")]
        public ProcesioProject Project { get; set; }
    }
}
