using Newtonsoft.Json;
using ProcesioSDK.Contracts;
using ProcesioSDK.Dto.Data;

namespace ProcesioSDK.Dto
{
    public class PublishResponse : IPublishResponse
    {
        [JsonProperty("flows")]
        public ProcesioProject Project { get; set; }
    }
}
