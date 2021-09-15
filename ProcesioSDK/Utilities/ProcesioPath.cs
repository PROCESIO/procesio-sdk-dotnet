using ProcesioSDK.Config;
using System;

namespace ProcesioSDK.Utilities
{
    internal static class ProcesioPath
    {
        public static Uri WebApiUrl(ProcesioConfig config)
        {
            return new Uri(string.Format(Constants.ProcesioURL, config.ServerName, config.MainPort));
        }

        public static Uri AuthenticationUrl(ProcesioConfig config)
        {
            return new Uri(string.Format(Constants.ProcesioURL, config.ServerName, config.AuthenticationPort));
        }
    }
}
