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
using static SDKProcesio.Responses.PublishResponse;

namespace SDKProcesio.Service
{
    public class SdkProcesio : ISdkProcesio
    {
        private readonly HttpClient client;

        public SdkProcesio()
        {
            client = new HttpClient();
        }

        public async Task<string> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (string.IsNullOrEmpty(id) || requestBody == null || string.IsNullOrEmpty(procesioTokens.AccessToken))
            {
                return null;
            }

            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new(ProcesioConfig.ProcesioURL);
            Uri uri = new(baseUri, string.Format(ProcesioConfig.ProcesioPublishMethod, id));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("workspace",workspace);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                var ceva = JsonConvert.DeserializeObject<Root>(response);
                return ceva.Flows?.Id?.ToString();
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> LaunchProjectInstance(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (string.IsNullOrEmpty(id) || requestBody == null)
            {
                return null;
            }
;
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new(ProcesioConfig.ProcesioURL);
            Uri uri = new(baseUri, string.Format(ProcesioConfig.ProcesioLaunchMethod, id));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("workspace", workspace);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LaunchResponse>(response).InstanceID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> RunProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (string.IsNullOrEmpty(id) || requestBody == null || string.IsNullOrEmpty(procesioTokens.AccessToken))
            {
                return null;
            }
           
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new(ProcesioConfig.ProcesioURL);
            Uri uri = new(baseUri, string.Format(ProcesioConfig.ProcesioRunMethod, id));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("workspace", workspace);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RunResponse>(response).InstanceID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<string> UploadFileFlow(UploadFileParam uploadFileParam, ProcesioTokens procesioTokens)
        {
            if (string.IsNullOrEmpty(uploadFileParam.FlowInstanceID)
                || string.IsNullOrEmpty(uploadFileParam.VariableName)
                || string.IsNullOrEmpty(uploadFileParam.FileID)
                || string.IsNullOrEmpty(uploadFileParam.RequestBody)
                || string.IsNullOrEmpty(procesioTokens.AccessToken))
            {
                return null;
            }


            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new(ProcesioConfig.ProcesioURL);
            Uri uri = new(baseUri, ProcesioConfig.ProcesioUploadFlowFile);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("fileId", uploadFileParam.FileID);
            client.DefaultRequestHeaders.Add("flowInstanceId", uploadFileParam.FlowInstanceID);
            client.DefaultRequestHeaders.Add("variableName", uploadFileParam.VariableName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(uploadFileParam.RequestBody, Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UploadResponse>(response).FileID;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        public async Task<ProcesioTokens> Authenticate(ProcesioUser procesioUser)
        {
            var queryString = new Dictionary<string, string>
            {
                { "realm", procesioUser.Realm },
                { "grant_type", procesioUser.GrantType },
                { "username", procesioUser.UserName },
                { "password", procesioUser.Password },
                { "client_id", procesioUser.ClientId }
            };

            Uri baseUri = new(ProcesioConfig.ProcesioAuthURL);
            Uri uri = new(baseUri, ProcesioConfig.ProcesioAuthMethod);

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

        public async Task<ProcesioTokens> RefreshToken(string refreshToken)
        {
            var queryString = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            Uri baseUri = new(ProcesioConfig.ProcesioAuthURL);
            Uri uri = new(baseUri, ProcesioConfig.ProcesioAuthMethod);

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

    }
}
