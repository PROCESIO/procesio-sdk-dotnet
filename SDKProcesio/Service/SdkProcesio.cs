using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SDKProcesio.Config;
using SDKProcesio.Responses;
using SDKProcesio.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SDKProcesio.Service
{
    public class SdkProcesio : ISdkProcesio
    {
        private readonly HttpClient client;
        private readonly ProcesioConfig _config;

        public SdkProcesio()
        {
            client = new HttpClient();
        }

        public SdkProcesio(IOptions<ProcesioConfig> config)
        {
            _config = config.Value;
        }

        public async Task<string> PublishProject(string projectId, string requestBody, string token)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(requestBody) || string.IsNullOrEmpty(token))
            {
                return null;
            }

            var procesioTokens = await GetProcesioTokens();
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(_config.ProcesioAuthClientId, procesioTokens.RefreshToken);
            }

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, string.Format(_config.ProcesioPublishMethod, projectId));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IPublishResponse>(response).FlowID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> LaunchFlowInstance(string flowId, string requestBody, string token)
        {
            if (string.IsNullOrEmpty(flowId) || string.IsNullOrEmpty(requestBody) || string.IsNullOrEmpty(token))
            {
                return null;
            }

            var procesioTokens = await GetProcesioTokens();
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(_config.ProcesioAuthClientId, procesioTokens.RefreshToken);
            }

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, string.Format(_config.ProcesioLaunchMethod, flowId));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ILaunchResponse>(response).InstanceID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> RunProject(string id, string requestBody, string token)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(requestBody) || string.IsNullOrEmpty(token))
            {
                return null;
            }

            var procesioTokens = await GetProcesioTokens();
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(_config.ProcesioAuthClientId, procesioTokens.RefreshToken);
            }

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, string.Format(_config.ProcesioRunMethod, id));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IRunResponse>(response).InstanceID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> UploadFileFlow(UploadFileParam uploadFileParam)
        {
            if (string.IsNullOrEmpty(uploadFileParam.FlowInstanceID)
                || string.IsNullOrEmpty(uploadFileParam.VariableName)
                || string.IsNullOrEmpty(uploadFileParam.FileID)
                || string.IsNullOrEmpty(uploadFileParam.RequestBody)
                || string.IsNullOrEmpty(uploadFileParam.Token))
            {
                return null;
            }

            var procesioTokens = await GetProcesioTokens();
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(_config.ProcesioAuthClientId, procesioTokens.RefreshToken);
            }

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, _config.ProcesioUploadFlowFile);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("fileId", uploadFileParam.FileID);
            client.DefaultRequestHeaders.Add("flowInstanceId", uploadFileParam.FlowInstanceID);
            client.DefaultRequestHeaders.Add("variableName", uploadFileParam.VariableName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", uploadFileParam.Token);

            var httpContent = new StringContent(uploadFileParam.RequestBody, Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IUploadResponse>(response).FileID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<ProcesioTokens> Authenticate(string realm, string grantType, string userName, string passw, string clientId)
        {
            var queryString = new Dictionary<string, string>
            {
                { "realm", realm },
                { "grant_type", grantType },
                { "username", userName },
                { "password", passw },
                { "client_id", clientId }
            };

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, _config.ProcesioAuthMethod);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await client.PostAsync(uri, new FormUrlEncodedContent(queryString));

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProcesioTokens>(response);
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<ProcesioTokens> RefreshToken(string clientId, string refreshToken)
        {
            var queryString = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            Uri baseUri = new(_config.ProcesioURL);
            Uri uri = new(baseUri, _config.ProcesioAuthURL);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await client.PostAsync(uri, new FormUrlEncodedContent(queryString));

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProcesioTokens>(response);
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<ProcesioTokens> GetProcesioTokens()
        {
            var queryString = new Dictionary<string, string>
            {
                { "realm", _config.ProcesioAuthRealm },
                { "grant_type", _config.ProcesioAuthGrantType },
                { "username", _config.ProcesioAuthUsername },
                { "password", _config.ProcesioAuthPassword },
                { "client_id", _config.ProcesioAuthClientId }
            };

            Uri baseUri = new(_config.ProcesioAuthURL);
            Uri uri = new(baseUri, _config.ProcesioAuthMethod);
            client.DefaultRequestHeaders.Clear();

            var httpResponse = await client.PostAsync(uri, new FormUrlEncodedContent(queryString));

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProcesioTokens>(response);
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }
    }
}
