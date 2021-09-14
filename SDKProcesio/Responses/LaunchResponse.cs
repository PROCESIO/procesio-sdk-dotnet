using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    public class LaunchResponse : ILaunchResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
