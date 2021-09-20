namespace ProcesioSDK.Config
{
    internal static class Constants
    {
        public const string PROCESIO_AUTH_METHOD = "/api/Authentication";
        public const string PROCESIO_URL = "https://{0}";
        public const string PROCESIO_RUN_METHOD = "/api/Projects/{0}/run";
        public const string PROCESIO_PUBLISH_METHOD = "/api/Projects/{0}/instances/publish";
        public const string PROCESIO_LAUNCH_METHOD = "/api/Projects/instances/{0}/launch";
        public const string PROCESIO_UPLOAD_FLOW_FILE = "/api/File/upload/flow";
        public const string PROCESIO_FLOW_STATE = "/api/Projects/instances/{0}/status";
        public const string PROCESIO_FILE_DATA_TYPE_ID = "10c6ac59-3929-49e6-99dc-121212121219";
        public const string PROCESIO_FILE_DATA_PROPERTY_ID = "id";
        public const string PROCESIO_FILE_DATA_PROPERTY_NAME = "name";

        public const string WORKSPACE = "workspace";
        public const string BEARER = "Bearer";
        public const string FILE_NAME = "FileName";
        public const string LENGTH = "Length";
        public const string RUN_SYNC = "runSynchronous";
        public const string SECONDS_TIMEOUT = "secondsTimeOut";
    }
}
