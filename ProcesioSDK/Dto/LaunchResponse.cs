using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The class used for the response of LaunchProjectInstance method, that returns the instance id
    /// </summary>
    public class LaunchResponse : ILaunchResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
