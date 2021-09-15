using Newtonsoft.Json;

namespace ProcesioSDK.Responses
{
    public class LaunchResponse : ILaunchResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
