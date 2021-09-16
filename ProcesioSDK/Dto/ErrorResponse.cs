using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    public class ErrorResponse : IErrorResponse
    {
        [JsonProperty("StatusCode")]
        public int StatusCode { get; set; } = -1;

        [JsonProperty("Target")]
        public string Target { get; set; } = string.Empty;

        [JsonProperty("Value")]
        public string Value { get; set; } = string.Empty;
    }
}
