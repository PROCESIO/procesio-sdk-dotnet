using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    public class RunResponse : IRunResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
