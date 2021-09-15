using Newtonsoft.Json;

namespace ProcesioSDK.Contracts
{
    public interface IUploadResponse
    {
        [JsonProperty("id")]
        string FileID { get; set; }
    }
}
