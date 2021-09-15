namespace ProcesioSDK.Config
{
    public class ProcesioConfig
    {
        public string ServerName { get; set; } = "api.procesio.app";
        public int MainPort { get; set; } = 4321;
        public int AuthenticationPort { get; set; } = 4532;
    }
}
