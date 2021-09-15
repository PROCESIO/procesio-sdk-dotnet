using Newtonsoft.Json;

namespace ProcesioSDK.Responses
{
    public class UploadResponse : IUploadResponse
    {
        [JsonProperty("id")]
        public string FileID { get; set; }
    }
}
