using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcesioSDK.Config;
using ProcesioSDK.Responses;
using ProcesioSDK.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProcesioSDK
{
    public class ProcesioClient : IProcesioClient
    {
        private readonly HttpClient _client;
        private readonly ProcesioConfig _procesioConfig;

        public Guid FlowId { get; set; }
        public Dictionary<string, Guid> FileMap { get; set; }
        public Dictionary<Guid,string> VariableNameMap { get; set; }

        /// <summary>
        /// Creates an instance of the Procesio SDK that can be used to access the main features provided by the Procesio App
        /// </summary>
        /// <param name="config">must have a ProcesioConfig section within appsettings file.</param>
        /// <exception cref="ArgumentNullException">This exception is thrown if the Procesio Configurationb is not valid</exception>
        public ProcesioClient(IConfiguration config)
        {
            _client = new HttpClient();
            _procesioConfig = config.GetValue<ProcesioConfig>("ProcesioConfig");
            ValidateProcesioConfiguration(_procesioConfig);
        }

        /// <summary>
        /// Creates an instance of the Procesio SDK that can be used to access the main features provided by the Procesio App
        /// </summary>
        /// <param name="procesioConfig">The Procesio Configuration required to access the Hosted Procesio server.</param>
        /// <exception cref="ArgumentNullException">This exception is thrown if the Procesio Configurationb is not valid</exception>
        public ProcesioClient(ProcesioConfig procesioConfig)
        {
            _procesioConfig = procesioConfig;
            ValidateProcesioConfiguration(_procesioConfig);
        }

        private void ValidateProcesioConfiguration(ProcesioConfig config)
        {
            if(string.IsNullOrEmpty(config.ServerName))
            {
                throw new ArgumentNullException("Server Name");
            }
            if(config.MainPort == 0)
            {
                throw new ArgumentNullException("Main Port");
            }
            if(config.AuthenticationPort == 0)
            {
                throw new ArgumentNullException("Authentication Port");
            }
        }

        public async Task<Flows> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (string.IsNullOrEmpty(id) 
                || requestBody == null 
                || string.IsNullOrEmpty(procesioTokens.AccessToken) 
                || string.IsNullOrEmpty(procesioTokens.RefreshToken))
            {
                return null;
            }

            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioPublishMethod, id));

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("workspace",workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);

            try
            {
               
                var response = await httpResponse.Content.ReadAsStringAsync();
                var responseDes = JsonConvert.DeserializeObject<Root>(response);
                FlowId = responseDes.Flows.Id;
                return responseDes.Flows;
   
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        
        public async Task<string> LaunchProjectInstance(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (id == null || requestBody == null || procesioTokens.AccessToken == null || procesioTokens.RefreshToken == null)
            {
                return null;
            }
;
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioLaunchMethod, id));

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("workspace", workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);

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
            if (id == null || requestBody == null || procesioTokens.AccessToken == null || procesioTokens.RefreshToken == null)
            {
                return null;
            }
           
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioRunMethod, id));

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("workspace", workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);

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

        public async Task<string> UploadFileFlow(UploadFileParam uploadFileParam, ProcesioTokens procesioTokens, string workspace)
        {
            if (uploadFileParam.FlowInstanceID == Guid.Empty || uploadFileParam.VariableName == null
                || procesioTokens.AccessToken == null 
                || procesioTokens.RefreshToken == null || uploadFileParam.FileId == Guid.Empty)
            {
                return null;
            }

            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), Constants.ProcesioUploadFlowFile);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("fileId", uploadFileParam.FileId.ToString());
            _client.DefaultRequestHeaders.Add("flowInstanceId", uploadFileParam.FlowInstanceID.ToString());
            _client.DefaultRequestHeaders.Add("variableName", uploadFileParam.VariableName);
            _client.DefaultRequestHeaders.Add("workspace", workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            MultipartFormDataContent form = new MultipartFormDataContent();
            using (var memoryStream = new MemoryStream())
            {
                uploadFileParam.FileContent.CopyTo(memoryStream);
                var fileByte =  memoryStream.ToArray();
                form.Add(new ByteArrayContent(fileByte, 0, fileByte.Length), uploadFileParam.FileName, uploadFileParam.FileName);
            }
            form.Add(new StringContent(uploadFileParam.FileName), nameof(uploadFileParam.FileName));
            form.Add(new StringContent(uploadFileParam.Length), nameof(uploadFileParam.Length));

            var httpResponse = await _client.PostAsync(uri, form);
            
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

        public void GetFileIds(Flows flow)
        {
            try
            {
                var flowFileTypeVariables = flow.Variables.Where(var => var.DataType.Equals(Constants.ProcesioFileDataTypeId)
                                                                        && var.Type == 10).ToList();
                var dic = new Dictionary<string, Guid>();
                var dic2 = new Dictionary<Guid, string>();

                foreach (var flowFileTypeVar in flowFileTypeVariables)
                {
                    if (flowFileTypeVar.IsList)
                    {
                        var fileModels = JArray.Parse(flowFileTypeVar.DefaultValue.ToString());
                        foreach (JObject fileModel in fileModels)
                        {
                            string fileName = fileModel.GetValue(Constants.ProcesioFileDataPropertyName)?.ToString();
                            Guid fileId = new Guid(fileModel.GetValue(Constants.ProcesioFileDataPropertyId)?.ToString());
                            dic.Add(fileName, fileId);
                            dic2.Add(fileId, flowFileTypeVar.Name);
                        }
                    }
                    else
                    {
                        var fileModel = JObject.Parse(flowFileTypeVar.DefaultValue.ToString());
                        string fileName = fileModel.GetValue(Constants.ProcesioFileDataPropertyName)?.ToString();
                        Guid fileId = new Guid(fileModel.GetValue(Constants.ProcesioFileDataPropertyId)?.ToString());
                        dic.Add(fileName, fileId);
                        dic2.Add(fileId, flowFileTypeVar.Name);
                    }
                }
                FileMap = dic;
                VariableNameMap = dic2;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }
        }

        public async Task<ProcesioTokens> Authenticate(ProcesioUser procesioUser)
        {
            if(procesioUser.Realm == null || procesioUser.GrantType == null || procesioUser.UserName == null
                || procesioUser.Password == null || procesioUser.ClientId == null)
            {
                return null;
            }

            var queryString = new Dictionary<string, string>
            {
                { "realm", procesioUser.Realm },
                { "grant_type", procesioUser.GrantType },
                { "username", procesioUser.UserName },
                { "password", procesioUser.Password },
                { "client_id", procesioUser.ClientId }
            };

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.ProcesioAuthMethod);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await _client.PostAsync(uri, new FormUrlEncodedContent(queryString));

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
            if(refreshToken == null)
            {
                return null;
            }

            var queryString = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.ProcesioAuthMethod);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await _client.PostAsync(uri, new FormUrlEncodedContent(queryString));

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
