using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    interface ILaunchResponse
    {
        [JsonProperty("instanceId")]
        string InstanceID { get; set; }
    }
}
