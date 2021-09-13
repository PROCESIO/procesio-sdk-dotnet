using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    interface IRunResponse
    {
        [JsonProperty("instanceId")]
        string InstanceID { get; set; }
    }
}
