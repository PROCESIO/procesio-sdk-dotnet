using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcesioSDK.Config;
using ProcesioSDK.Dto;
using ProcesioSDK.Dto.Data;
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
    /// <summary>
    /// Implements the methods that authenticate user by credentials; refresh user token valability; 
    /// publish the process to new runtime instance and launch flow instance; run process; upload file used for flows
    /// </summary>
    public class ProcesioClient : IProcesioClient
    {
        private readonly HttpClient _client;
        private readonly ProcesioConfig _procesioConfig;

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
            if (procesioUser.GrantType == null
                || procesioUser.UserName == null
                || procesioUser.Password == null)
            {
                return null;
            }

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.PROCESIO_AUTH_METHOD);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

            var httpResponse = await _client.PostAsync(uri, new FormUrlEncodedContent(procesioUser.GetAuthenticationInformation(_procesioConfig)));

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

            Uri uri = new Uri(ProcesioPath.AuthenticationUrl(_procesioConfig), Constants.PROCESIO_AUTH_METHOD);

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
        public async Task<Response<ProcessInstance>> PublishProcess(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processId))
            {
                throw new Exception("Empty process id.");
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

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_PUBLISH_METHOD, processId));
            InitBasicClientHeaders(procesioToken, workspace);

            var serializedInputValues = JsonConvert.SerializeObject(inputValues);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToProcessInstance(response);
        }

        /// <inheritdoc />
        public async Task<Response<LaunchResponse>> LaunchProcessInstance(string processInstanceId, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processInstanceId))
            {
                throw new Exception("Empty process instance id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token!");
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_LAUNCH_METHOD, processInstanceId));
            InitBasicClientHeaders(procesioToken, workspace);

            var httpContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToLaunchResult(response);
        }

        /// <inheritdoc />
        public async Task<Response<LaunchResponse>> RunProcess(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processId))
            {
                throw new Exception("Empty process instance id.");
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

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_RUN_METHOD, processId));
            InitBasicClientHeaders(procesioToken, workspace);

            var serializedInputValues = JsonConvert.SerializeObject(inputValues);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToLaunchResult(response);
        }

        /// <inheritdoc />
        public async Task<Response<UploadResponse>> UploadInputFileToProcessInstance(string processInstanceId, ProcesioFileContent fileContent, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processInstanceId))
            {
                throw new Exception("Empty process instance id.");
            }
            else if (procesioToken == null
                || string.IsNullOrEmpty(procesioToken.AccessToken)
                || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token.");
            }
            else if (string.IsNullOrEmpty(fileContent.VariableName))
            {
                throw new Exception("Empty variable name.");
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), Constants.PROCESIO_UPLOAD_FLOW_FILE);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("fileId", fileContent.FileContent.FileId.ToString());
            _client.DefaultRequestHeaders.Add("flowInstanceId", processInstanceId);
            _client.DefaultRequestHeaders.Add("variableName", fileContent.VariableName);
            _client.DefaultRequestHeaders.Add(Constants.WORKSPACE, workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BEARER, procesioToken.AccessToken);

            MultipartFormDataContent form = new MultipartFormDataContent();
            using (var memoryStream = new MemoryStream())
            {
                fileContent.FileContent.Data.CopyTo(memoryStream);
                var fileByte = memoryStream.ToArray();
                form.Add(new ByteArrayContent(fileByte, 0, fileByte.Length), Constants.FILE_NAME, fileContent.FileContent.Name);
                form.Add(new StringContent(fileContent.FileContent.Name), Constants.FILE_NAME);
                form.Add(new StringContent(fileByte.Length.ToString()), Constants.LENGTH);
            }

            var httpResponse = await _client.PostAsync(uri, form);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToUploadResponse(response);
        }
        
        /// <inheritdoc />
        public IEnumerable<ProcesioFileContent> GetInputFileInfo(ProcessInstance process)
        {
            var result = new List<ProcesioFileContent>();
            try
            {
                var flowFileTypeVariables = process.Variables.Where(var => var.DataType.Equals(Constants.PROCESIO_FILE_DATA_TYPE_ID)
                                                                        && var.Type == 10).ToList();

                foreach (var flowFileTypeVar in flowFileTypeVariables)
                {
                    if (flowFileTypeVar.IsList)
                    {
                        var fileList = JsonConvert.DeserializeObject<IEnumerable<FileContent>>(flowFileTypeVar.DefaultValue.ToString());
                        foreach(var fileItem in fileList)
                        {
                            result.Add(new ProcesioFileContent(flowFileTypeVar.Name, fileItem));
                        }
                    }
                    else
                    {
                        var fileItem = JsonConvert.DeserializeObject<FileContent>(flowFileTypeVar.DefaultValue.ToString());
                        result.Add(new ProcesioFileContent(flowFileTypeVar.Name, fileItem));
                    }
                }
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return result;
        }


        private void ValidateProcesioConfiguration(ProcesioConfig config)
        {
            if (string.IsNullOrEmpty(config.ServerUri))
            {
                throw new ArgumentNullException("Server Uri");
            }
            if (string.IsNullOrEmpty(config.AuthenticationUri))
            {
                throw new ArgumentNullException("Authentication Uri");
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
            _client.DefaultRequestHeaders.Add(Constants.WORKSPACE, workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BEARER, procesioToken.AccessToken);
        }

        private Response<ProcessInstance> ConvertToProcessInstance(string response)
        {
            try
            {
                var responseDes = JsonConvert.DeserializeObject<PublishResponse>(response);
                return new Response<ProcessInstance>(responseDes.Process, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<ErrorResponse>(response);
                return new Response<ProcessInstance>(null, responseDes);
            }
            catch
            {
                Console.WriteLine("HTTP Response is not of type ErrorResponse.");
            }

            Console.WriteLine($"Error in converting the Publish process result: {response}");

            return null;
        }

        private Response<LaunchResponse> ConvertToLaunchResult(string response)
        {
            try
            {
                var responseDes = JsonConvert.DeserializeObject<LaunchResponse>(response);
                return new Response<LaunchResponse>(responseDes, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<ErrorResponse>(response);
                return new Response<LaunchResponse>(null, responseDes);
            }
            catch
            {
                Console.WriteLine("HTTP Response is not of type ErrorResponse.");
            }

            Console.WriteLine($"Error in converting the Launch process result: {response}");

            return null;
        }

        private Response<UploadResponse> ConvertToUploadResponse(string response)
        {
            try
            {
                var responseDes = JsonConvert.DeserializeObject<UploadResponse>(response);
                return new Response<UploadResponse>(responseDes, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<ErrorResponse>(response);
                return new Response<UploadResponse>(null, responseDes);
            }
            catch
            {
                Console.WriteLine("HTTP Response is not of type ErrorResponse.");
            }

            Console.WriteLine($"Error in converting the Launch process result: {response}");

            return null;
        }

    }
}
