using System;
using System.IO;

namespace ProcesioSDK.Dto
{
    public class ProcesioFileContent
    {
        public string FilePath { get; set; }
        public string VariableName { get; set; }
        public Guid FileId { get; set; }
        public string Length { get; set; }
        public FileContent FileData { get; set; }

        public ProcesioFileContent(string variableName, Guid fileId, Stream fileContent, string filePath)
        {
            FilePath = filePath;
            VariableName = variableName;
            FileId = fileId;

            FileData = new FileContent()
            {
                Data = fileContent,
                Name = Path.GetFileName(filePath)
            };
        }
    }
}
