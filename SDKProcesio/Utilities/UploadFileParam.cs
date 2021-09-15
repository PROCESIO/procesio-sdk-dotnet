using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace SDKProcesio.Utilities
{
    public class UploadFileParam
    {
        public Guid FlowInstanceID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string VariableName { get; set; }
        public Stream FileContent { get; set; }
        public Guid FileId { get; set; }
        public string Length { get; set; }

        public UploadFileParam(Guid flowInstanceID, string variableName, Guid fileId, Stream fileContent, string filePath)
        {
            FlowInstanceID = flowInstanceID;
            FilePath = filePath;
            VariableName = variableName;
            FileContent = fileContent;
            FileId = fileId;
            FileName = Path.GetFileName(filePath);
        }
    }
}
