using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    public class UploadResponse : IUploadResponse
    {
        [JsonProperty("id")]
        public string FileID { get; set; }
    }
}
