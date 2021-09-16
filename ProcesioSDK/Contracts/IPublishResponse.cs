using Newtonsoft.Json;
using ProcesioSDK.Dto.Data;

namespace ProcesioSDK.Contracts
{
    public interface IPublishResponse
    {
        [JsonProperty("flows")]
        ProcessInstance Process { get; set; }
    }
}
