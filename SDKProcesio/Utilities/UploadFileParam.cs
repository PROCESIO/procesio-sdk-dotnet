namespace SDKProcesio.Utilities
{
    public class UploadFileParam
    {
        public string FlowInstanceID { get; set; }
        public string VariableName { get; set; }
        public string FileID { get; set; }
        public string RequestBody { get; set; }

        public UploadFileParam() { }
        public UploadFileParam(string flowInstanceID, string variableName, string fileID, string requestBody)
        {
            FlowInstanceID = flowInstanceID;
            VariableName = variableName;
            FileID = fileID;
            RequestBody = requestBody;
        }
    }
}
