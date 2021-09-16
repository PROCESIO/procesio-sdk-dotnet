using Newtonsoft.Json;
using ProcesioSDK.Contracts;
using ProcesioSDK.Dto.Data;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The class used for the response of Publish Process method, that returns the variables, flow id, description,
    /// title, first name, last name, workspace name of the process
    /// </summary>
    public class PublishResponse : IPublishResponse
    {
        [JsonProperty("flows")]
        public ProcessInstance Process { get; set; }
    }
}
