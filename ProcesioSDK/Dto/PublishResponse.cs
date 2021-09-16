using Newtonsoft.Json;
using ProcesioSDK.Contracts;
using ProcesioSDK.Dto.Data;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The class used for the response of PublishProject method, that returns the variables, flow id, description,
    /// title, first name, last name, workspace name of the project
    /// </summary>
    public class PublishResponse : IPublishResponse
    {
        [JsonProperty("flows")]
        public ProcesioProject Project { get; set; }
    }
}
