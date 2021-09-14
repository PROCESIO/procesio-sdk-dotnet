
using System.Configuration;

namespace SDKProcesio.Config
{
	public static class ProcesioConfig
	{
		public static string ProcesioAuthURL { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioAuthURL");
		public static string ProcesioAuthMethod { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioAuthMethod");
		public static string ProcesioURL { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioURL");
		public static string ProcesioRunMethod { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioRunMethod");
		public static string ProcesioPublishMethod { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioPublishMethod");
		public static string ProcesioLaunchMethod { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioLaunchMethod");
		public static string ProcesioUploadFlowFile { get; set; } = ConfigurationManager.AppSettings.Get("ProcesioUploadFlowFile");
	}
}
