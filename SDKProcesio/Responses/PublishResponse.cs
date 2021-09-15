
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SDKProcesio.Responses
{
      public class Variable
     {
        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("isList")]
        public bool IsList { get; set; }

        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }
     }

    public class Flows
    {
        [JsonProperty("variables")]
        public List<Variable> Variables { get; set; }

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
    }

    public class Root
    {
        [JsonProperty("flows")]
        public Flows Flows { get; set; }
    }
}
