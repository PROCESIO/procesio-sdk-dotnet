namespace ProcesioSDK.Dto.Data
{
    public class ProcesioFileContent
    {
        public string VariableName { get; set; }
        public FileContent FileContent { get; set; }

        public ProcesioFileContent(string variableName, FileContent fileContent)
        {
            VariableName = variableName;
            FileContent = fileContent;
        }
    }
}
