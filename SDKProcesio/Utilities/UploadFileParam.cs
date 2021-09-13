namespace SDKProcesio.Utilities
{
    public class UploadFileParam
    {
        public string FlowInstanceID { get; set; }
        public string VariableName { get; set; }
        public string FileID { get; set; }
        public string RequestBody { get; set; }
        public string Token { get; set; }
        public UploadFileParam(string flowInstanceID, string variableName, string fileID, string requestBody, string token)
        {
            FlowInstanceID = flowInstanceID;
            VariableName = variableName;
            FileID = fileID;
            RequestBody = requestBody;
            Token = token;
        }
    }
}
