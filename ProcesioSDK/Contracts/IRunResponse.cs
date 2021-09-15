using Newtonsoft.Json;

namespace ProcesioSDK.Contracts
{
    public interface IRunResponse
    {
        [JsonProperty("instanceId")]
        string InstanceID { get; set; }
    }
}
