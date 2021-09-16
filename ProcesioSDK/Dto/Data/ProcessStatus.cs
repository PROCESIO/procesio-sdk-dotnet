using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProcesioSDK.Dto.Data
{
    internal class ProcessStatus
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("variable")]
        public Dictionary<string, object> Variable { get; set; } = new Dictionary<string, object>();
    }
}
