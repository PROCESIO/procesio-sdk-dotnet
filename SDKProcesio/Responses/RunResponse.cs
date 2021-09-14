using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    public class RunResponse : IRunResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
