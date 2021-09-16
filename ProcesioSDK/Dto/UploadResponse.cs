using Newtonsoft.Json;
using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The class used for the response of UploadInputFileToProject method, that returns the file id
    /// </summary>
    public class UploadResponse : IUploadResponse
    {
        [JsonProperty("id")]
        public string FileID { get; set; }
    }
}
