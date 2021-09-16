using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcesioSDK.Config;
using ProcesioSDK.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ProcesioSDK.Utilities;
using ProcesioSDK.Dto.Data;

namespace ProcesioSDK
{
    /// <summary>
    /// Implements the methods that authenticate user by credentials; refresh user token valability; 
    /// publish the project to new runtime instance and launch flow instance; run project; upload file used for flows
    /// </summary>
    public class ProcesioClient : IProcesioClient
    {
        private readonly HttpClient _client;
        private readonly ProcesioConfig _procesioConfig;

        public Guid FlowId { get; set; }
        public Dictionary<string, Guid> FileMap { get; set; }
        public Dictionary<Guid, string> VariableNameMap { get; set; }

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

        /// <inheritdoc />
        public async Task<ProcesioToken> Authenticate(ProcesioUser procesioUser)
        {
            if (procesioUser.Realm == null || procesioUser.GrantType == null || procesioUser.UserName == null
                || procesioUser.Password == null || procesioUser.ClientId == null)
            {
                return null;
            }

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.ProcesioAuthMethod);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await _client.PostAsync(uri, new FormUrlEncodedContent(procesioUser.GetAuthenticationInformation()));

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProcesioToken>(response);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Authentication HTTP Response content is invalid or cannot be deserialised.", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RefreshToken(ProcesioToken procesioToken)
        {
            if (procesioToken == null || procesioToken.RefreshToken == null)
            {
                return false;
            }

            var queryString = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", procesioToken.RefreshToken }
            };

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.ProcesioAuthMethod);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await _client.PostAsync(uri, new FormUrlEncodedContent(queryString));

            try
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                var newToken = JsonConvert.DeserializeObject<ProcesioToken>(response);
                if (newToken == null) return false;
                procesioToken = newToken; // update the token
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Authentication HTTP Response content is invalid or cannot be deserialised.");
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<ProcesioProject> PublishProject(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Empty project id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token!");
            }

            if (inputValues == null)
            {
                inputValues = new Dictionary<string, object>();
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioPublishMethod, id));
            InitBasicClientHeaders(procesioToken, workspace);

            var serializedInputValues = JsonConvert.SerializeObject(inputValues);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);

            try
            {

                var response = await httpResponse.Content.ReadAsStringAsync();
                var responseDes = JsonConvert.DeserializeObject<PublishResponse>(response);
                FlowId = responseDes.Project.Id;
                return responseDes.Project;
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<string> LaunchProjectInstance(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Empty project instance id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token!");
            }

            if (inputValues == null)
            {
                inputValues = new Dictionary<string, object>();
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioLaunchMethod, id));
            InitBasicClientHeaders(procesioToken, workspace);

            var serializedInputValues = JsonConvert.SerializeObject(inputValues);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
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

        /// <inheritdoc />
        public async Task<string> RunProject(string id, Dictionary<string, object> inputValues, string workspace, ProcesioToken procesioToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Empty project instance id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token!");
            }

            if (inputValues == null)
            {
                inputValues = new Dictionary<string, object>();
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.ProcesioRunMethod, id));
            InitBasicClientHeaders(procesioToken, workspace);

            var serializedInputValues = JsonConvert.SerializeObject(inputValues);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
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

        /// <inheritdoc />
        public async Task<string> UploadInputFileToProject(string id, ProcesioFileContent fileContent, ProcesioToken procesioToken, string workspace)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Empty project instance id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token.");
            }
            else if(string.IsNullOrEmpty(fileContent.VariableName))
            {
                throw new Exception("Empty variable name.");
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), Constants.ProcesioUploadFlowFile);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("fileId", fileContent.FileId.ToString());
            _client.DefaultRequestHeaders.Add("flowInstanceId", id);
            _client.DefaultRequestHeaders.Add("variableName", fileContent.VariableName);
            _client.DefaultRequestHeaders.Add("workspace", workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioToken.AccessToken);

            MultipartFormDataContent form = new MultipartFormDataContent();
            using (var memoryStream = new MemoryStream())
            {
                fileContent.FileData.Data.CopyTo(memoryStream);
                var fileByte = memoryStream.ToArray();
                form.Add(new ByteArrayContent(fileByte, 0, fileByte.Length), fileContent.FileData.Name, fileContent.FileData.Name);
            }
            form.Add(new StringContent(fileContent.FileData.Name), nameof(fileContent.FileData.Name));
            form.Add(new StringContent(fileContent.Length), nameof(fileContent.Length));

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

        /// <inheritdoc />
        public void GetFileIds(ProcesioProject project)
        {
            try
            {
                var flowFileTypeVariables = project.Variables.Where(var => var.DataType.Equals(Constants.ProcesioFileDataTypeId)
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


        private void ValidateProcesioConfiguration(ProcesioConfig config)
        {
            if (string.IsNullOrEmpty(config.ServerName))
            {
                throw new ArgumentNullException("Server Name");
            }
            if (config.MainPort == 0)
            {
                throw new ArgumentNullException("Main Port");
            }
            if (config.AuthenticationPort == 0)
            {
                throw new ArgumentNullException("Authentication Port");
            }
            if (string.IsNullOrEmpty(config.AuthenticationClientId))
            {
                throw new ArgumentNullException("Authentication Client Id");
            }
            if (string.IsNullOrEmpty(config.AuthenticationRealm))
            {
                throw new ArgumentNullException("Authentication Realm");
            }
        }

        private void InitBasicClientHeaders(ProcesioToken procesioToken, string workspace)
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("workspace", workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioToken.AccessToken);
        }
    }
}
