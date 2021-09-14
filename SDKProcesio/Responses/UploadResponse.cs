using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    public class UploadResponse : IUploadResponse
    {
        [JsonProperty("id")]
        public string FileID { get; set; }
    }
}
