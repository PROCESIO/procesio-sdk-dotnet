using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The response object for the started process instance
    /// </summary>
    public class ProcessStatusResponse
    {
        /// <summary>
        /// Process state
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Variable list value
        /// </summary>
        public Dictionary<string, object> Variable { get; set; } = new Dictionary<string, object>();
    }
}
