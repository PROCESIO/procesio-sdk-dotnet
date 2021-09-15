using Newtonsoft.Json;

namespace ProcesioSDK.Contracts
{
    public interface ILaunchResponse
    {
        [JsonProperty("instanceId")]
        string InstanceID { get; set; }
    }
}
