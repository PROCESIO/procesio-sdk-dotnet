using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    interface IPublishResponse
    {
        [JsonProperty("flowId")]
        string FlowID { get; set; }
    }
}
