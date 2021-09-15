using Newtonsoft.Json;

namespace ProcesioSDK.Responses
{
    public class RunResponse : IRunResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
