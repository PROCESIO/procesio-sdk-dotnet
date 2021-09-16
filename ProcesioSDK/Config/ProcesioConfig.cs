namespace ProcesioSDK.Config
{
    public class ProcesioConfig
    {
        public string ServerUri { get; set; } = "api.procesio.app:4321";
        public string AuthenticationUri { get; set; } = "api.procesio.app:4532";
        public string AuthenticationRealm { get; set; } 
        public string AuthenticationClientId { get; set; } 
    }
}
