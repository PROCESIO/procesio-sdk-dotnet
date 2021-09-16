using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ProcesioSDK.Dto.Data
{
    public class ProcessInstance
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isValid")]
        public bool IsValid { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("workspaceName")]
        public string WorkspaceName { get; set; }

        [JsonProperty("variables")]
        public List<ProcessVariable> Variables { get; set; }
    }
}
