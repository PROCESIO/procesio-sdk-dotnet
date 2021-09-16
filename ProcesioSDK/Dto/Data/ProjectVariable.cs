using Newtonsoft.Json;

namespace ProcesioSDK.Dto.Data
{
    public class ProjectVariable
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
}
