using ProcesioSDK.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcesioSDK.Utilities
{
    internal static class ProcesioPath
    {
        public static Uri WebApiUrl(ProcesioConfig config)
        {
            return new Uri(string.Format("http://{0}", config.ServerUri));
        }

        public static Uri AuthenticationUrl(ProcesioConfig config)
        {
            return new Uri(string.Format(Constants.PROCESIO_URL, config.AuthenticationUri));
        }

        public static Uri SetUriQueryParameters(Uri uri, Dictionary<string, string> queryParams)
        {
            if (queryParams.Count <= 0)
            {
                return uri;
            }
            var formatedQuery = string.Format("?{0}", string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}").ToArray()));
            return new Uri(uri, formatedQuery);
        }
    }
}
