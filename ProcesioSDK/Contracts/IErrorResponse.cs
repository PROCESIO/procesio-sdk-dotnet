using Newtonsoft.Json;

namespace ProcesioSDK.Contracts
{
    public interface IErrorResponse
    {
        [JsonProperty("StatusCode")]
        int StatusCode { get; set; } 

        [JsonProperty("Target")]
        string Target { get; set; }

        [JsonProperty("Value")]
        string Value { get; set; }
    }
}
