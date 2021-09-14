
using Newtonsoft.Json;

namespace SDKProcesio.Responses
{
    public class PublishResponse 
    {
        public class Flows
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public int Status { get; set; }

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
}
