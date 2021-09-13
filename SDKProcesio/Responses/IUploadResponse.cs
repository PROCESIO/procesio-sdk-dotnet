using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    interface IUploadResponse
    {
        [JsonProperty("id")]
        string FileID { get; set; }
    }
}
