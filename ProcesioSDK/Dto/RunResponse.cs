using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The class used for the response of RunProject method, that returns the instance id
    /// </summary>
    public class RunResponse : IRunResponse
    {
        [JsonProperty("instanceId")]
        public string InstanceID { get; set; }
    }
}
