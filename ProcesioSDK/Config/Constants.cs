namespace ProcesioSDK.Config
{
    internal static class Constants
    {
        public const string ProcesioAuthMethod = "/api/Authentication";
        public const string ProcesioURL = "https://{0}:{1}";
        public const string ProcesioRunMethod = "/api/Projects/{0}/run";
        public const string ProcesioPublishMethod = "/api/Projects/{0}/instances/publish";
        public const string ProcesioLaunchMethod = "/api/Projects/instances/{0}/launch";
        public const string ProcesioUploadFlowFile = "/api/File/upload/flow";
        public const string ProcesioFileDataTypeId = "10c6ac59-3929-49e6-99dc-121212121219";
        public const string ProcesioFileDataPropertyId = "id";
        public const string ProcesioFileDataPropertyName = "name";
    }
}
