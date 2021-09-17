using System.Collections.Generic;

namespace ProcesioSDK.Dto
{
    /// <summary>
    /// The response object for the started process instance
    /// </summary>
    public class ProcessStatusResponse
    {
        /// <summary>
        /// The process instance id
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Process state
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// Process state description
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Variable list value
        /// </summary>
        public Dictionary<string, object> Variable { get; set; } = new Dictionary<string, object>();
    }
}
