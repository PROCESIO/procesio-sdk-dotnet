using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    public class LaunchResponse : ILaunchResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
