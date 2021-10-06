using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ProcesioSDK.Config;
using ProcesioSDK.Dto;
using ProcesioSDK.Dto.Data;
using ProcesioSDK.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProcesioSDK
{
    /// <summary>
    /// Procesio Client used to communicate with the Procesio web services
    /// </summary>
    public class ProcesioClient : IProcesioClient
    {
        private readonly HttpClient _client;
        private readonly ProcesioConfig _procesioConfig;

        #region Constructors
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
            _client = new HttpClient();
            _procesioConfig = procesioConfig;
            ValidateProcesioConfiguration(_procesioConfig);
        }
        #endregion

        #region Authentication
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
            var response = await httpResponse.Content.ReadAsStringAsync();

            var responseToken = new ProcesioToken();
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                responseToken = JsonConvert.DeserializeObject<ProcesioToken>(response);
            }
            responseToken.StatusCode = httpResponse.StatusCode;
            return responseToken;
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
        #endregion

        #region Publish+Upload+Launch

        /// <inheritdoc />
        public async Task<Response<ProcessInstance>> PublishProcess(
            string processId,
            Dictionary<string, object> inputValues,
            ProcesioToken procesioToken,
            string workspace = null)
        {
            if (string.IsNullOrEmpty(processId))
            {
                throw new Exception("Empty process id.");
            }
            await ValidateAuthentication(procesioToken);

            if (inputValues == null)
            {
                inputValues = new Dictionary<string, object>();
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
        public IEnumerable<FileContent> GetProcessInputFileContent(ProcessInstance process)
        {
            var result = new List<FileContent>();
            try
            {
                var flowFileTypeVariables = process.Variables.Where(var => var.DataType.Equals(Constants.PROCESIO_FILE_DATA_TYPE_ID)
                                                                        && var.Type == 10).ToList();

                foreach (var flowFileTypeVar in flowFileTypeVariables)
                {
                    if (flowFileTypeVar.IsList)
                    {
                        var fileList = JsonConvert.DeserializeObject<IEnumerable<FileContent>>(flowFileTypeVar.DefaultValue.ToString());
                        foreach (var fileItem in fileList)
                        {
                            fileItem.VariableName = flowFileTypeVar.Name;
                            result.Add(fileItem);
                        }
                    }
                    else
                    {
                        var fileItem = JsonConvert.DeserializeObject<FileContent>(flowFileTypeVar.DefaultValue.ToString());
                        fileItem.VariableName = flowFileTypeVar.Name;
                        result.Add(fileItem);
                    }
                }
            }
            catch
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task UploadProcessInputFiles(ProcessInstance processInstance, List<FileContent> inputFiles, ProcesioToken procesioToken, string workspace = null)
        {
            var fileListToUpload = GetProcessInputFileContent(processInstance);
            UpdateFileInfo(inputFiles, fileListToUpload);

            foreach (var fileInput in fileListToUpload)
            {
                var fileUploadResponse = await UploadProcessInputFiles(processInstance, fileInput, procesioToken, workspace);
                if (!fileUploadResponse.IsSuccess || !fileUploadResponse.Content.FileID.Equals(fileInput.FileId.ToString()))
                {
                    var errorResponse = JsonConvert.SerializeObject(fileUploadResponse.Error);
                    throw new ProcessFileUploadException(errorResponse);
                }
            }
        }

        /// <inheritdoc />
        public async Task<Response<UploadResponse>> UploadProcessInputFiles(ProcessInstance processInstance, FileContent inputFile, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processInstance.Id.ToString()))
            {
                throw new Exception("Empty process instance id.");
            }
            else if (string.IsNullOrEmpty(inputFile.VariableName))
            {
                throw new Exception("Empty variable name.");
            }
            await ValidateAuthentication(procesioToken);

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), Constants.PROCESIO_UPLOAD_FLOW_FILE);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("fileId", inputFile.FileId.ToString());
            _client.DefaultRequestHeaders.Add("flowInstanceId", processInstance.Id.ToString());
            _client.DefaultRequestHeaders.Add("variableName", inputFile.VariableName);
            _client.DefaultRequestHeaders.Add(Constants.WORKSPACE, workspace);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BEARER, procesioToken.AccessToken);

            using (var form = new MultipartFormDataContent())
            {
                using (var streamContent = new StreamContent(inputFile.Data))
                {
                    using (var fileByteArray = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                    {
                        fileByteArray.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                        form.Add(fileByteArray, Constants.FILE_NAME);

                        streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "package",
                            FileName = inputFile.Name
                        };
                        streamContent.Headers.Remove("Content-Type");
                        form.Add(streamContent);

                        var httpResponse = await _client.PostAsync(uri, form);
                        var response = await httpResponse.Content.ReadAsStringAsync();
                        return ConvertToUploadResponse(response);
                    }
                }
            }
        }


        /// <inheritdoc />
        public async Task<Response<LaunchResponse>> LaunchProcessInstance(string processInstanceId, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processInstanceId))
            {
                throw new Exception("Empty process instance id.");
            }
            await ValidateAuthentication(procesioToken);

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_LAUNCH_METHOD, processInstanceId));
            InitBasicClientHeaders(procesioToken, workspace);

            var payload = new
            {
                ConnectionId = ""
            };
            var serializedInputValues = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToLaunchResult(response);
        }

        /// <inheritdoc />
        public async Task<Response<ProcessStatusResponse>> LaunchProcessInstance(string processInstanceId, ProcesioToken procesioToken, string workspace = null, int timeOut = 60)
        {
            if (string.IsNullOrEmpty(processInstanceId))
            {
                throw new Exception("Empty process instance id.");
            }
            await ValidateAuthentication(procesioToken);
            Uri uri = BuildSyncUri(Constants.PROCESIO_LAUNCH_METHOD, processInstanceId, timeOut);

            InitBasicClientHeaders(procesioToken, workspace);

            var payload = new
            {
                ConnectionId = ""
            };
            var serializedInputValues = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(uri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToRunProcessResponse(response);
        }

        #endregion

        #region Run

        /// <inheritdoc />
        public async Task<Response<LaunchResponse>> RunProcess(
            string processId,
            Dictionary<string, object> inputValues,
            ProcesioToken procesioToken,
            string workspace = null,
            List<FileContent> inputFiles = null)
        {
            inputValues = await ValidateInput(processId, inputValues, procesioToken);

            Uri runUri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_RUN_METHOD, processId));

            if (inputFiles == null || !inputFiles.Any())
            {
                string response = await RunProcess(inputValues, procesioToken, workspace, runUri);
                return ConvertToLaunchResult(response);
            }

            // else we need to upload the input files after publish
            var processInstance = await PublishProcess(processId, inputValues, procesioToken, workspace);
            if (!processInstance.IsSuccess)
            {
                var errorResponse = JsonConvert.SerializeObject(processInstance.Error);
                throw new ProcessPublishException(errorResponse);
            }

            await UploadProcessInputFiles(processInstance.Content, inputFiles, procesioToken, workspace);

            return await LaunchProcessInstance(processInstance.Content.Id.ToString(), procesioToken, workspace);
        }

        /// <inheritdoc />
        public async Task<Response<ProcessStatusResponse>> RunProcess(
            string processId,
            Dictionary<string, object> inputValues,
            ProcesioToken procesioToken,
            string workspace = null,
            List<FileContent> inputFiles = null,
            int timeOut = 60)
        {
            inputValues = await ValidateInput(processId, inputValues, procesioToken);

            Uri runUri = BuildSyncUri(Constants.PROCESIO_RUN_METHOD, processId, timeOut);

            if (inputFiles == null || !inputFiles.Any())
            {
                string response = await RunProcess(inputValues, procesioToken, workspace, runUri);
                return ConvertToRunProcessResponse(response);
            }

            // else we need to upload the input files after publish
            var processInstance = await PublishProcess(processId, inputValues, procesioToken, workspace);
            if (!processInstance.IsSuccess)
            {
                var errorResponse = JsonConvert.SerializeObject(processInstance.Error);
                throw new ProcessPublishException(errorResponse);
            }

            await UploadProcessInputFiles(processInstance.Content, inputFiles, procesioToken, workspace);

            return await LaunchProcessInstance(processInstance.Content.Id.ToString(), procesioToken, workspace, timeOut);// ------
        }
        #endregion

        #region ProcessStatus

        /// <inheritdoc />
        public async Task<Response<ProcessStatusResponse>> GetProcessStatus(string processInstanceId, ProcesioToken procesioToken, string workspace = null)
        {
            if (string.IsNullOrEmpty(processInstanceId))
            {
                throw new Exception("Empty process instance id.");
            }
            await ValidateAuthentication(procesioToken);

            Uri uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(Constants.PROCESIO_FLOW_STATE, processInstanceId));
            InitBasicClientHeaders(procesioToken, workspace);

            var httpResponse = await _client.GetAsync(uri);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return ConvertToProcessResponse(response, processInstanceId);
        }
        #endregion




        #region Private Methods
        private async Task ValidateAuthentication(ProcesioToken procesioToken)
        {
            if (procesioToken == null
                            || string.IsNullOrEmpty(procesioToken.AccessToken)
                            || string.IsNullOrEmpty(procesioToken.RefreshToken))
            {
                throw new Exception("Invalid authentication token!");
            }

            if (procesioToken.Expires_in <= 0)
            {
                await RefreshToken(procesioToken);
            }
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



        private void UpdateFileInfo(IEnumerable<FileContent> inputFiles, IEnumerable<FileContent> variableInformation)
        {
            foreach (var varFile in variableInformation)
            {
                var inputFile = inputFiles.FirstOrDefault(i => i.VariableName.Equals(varFile.VariableName) && i.Name.Equals(varFile.Name));
                if (inputFile == null)
                {
                    throw new Exception("Input file list doesn't match with process instance variables");
                }
                varFile.Data = inputFile.Data;
            }
        }

        private Uri BuildSyncUri(string baseUrl, string processInstanceId, int timeOut)
        {
            var queryParams = new Dictionary<string, string>()
            {
                { Constants.RUN_SYNC, true.ToString() },
                { Constants.SECONDS_TIMEOUT, timeOut.ToString() }
            };
            var uri = new Uri(ProcesioPath.WebApiUrl(_procesioConfig), string.Format(baseUrl, processInstanceId));
            uri = ProcesioPath.SetUriQueryParameters(uri, queryParams);
            return uri;
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
            if (Guid.TryParse(response, out Guid responseGuid))
            {
                var responseDes = new UploadResponse() { FileID = response };
                return new Response<UploadResponse>(responseDes, new ErrorResponse());
            }

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

        private Response<ProcessStatusResponse> ConvertToProcessResponse(string response, string processInstanceId)
        {
            try
            {
                var responseDes = JsonConvert.DeserializeObject<ProcessStatus>(response);
                var result = new ProcessStatusResponse()
                {
                    Variable = responseDes.Variable,
                    State = responseDes.Status,
                    Status = ProcesioConverters.ConvertProcessStatus(responseDes.Status),
                    InstanceId = processInstanceId
                };
                return new Response<ProcessStatusResponse>(result, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<ErrorResponse>(response);
                return new Response<ProcessStatusResponse>(null, responseDes);
            }
            catch
            {
                Console.WriteLine("HTTP Response is not of type ErrorResponse.");
            }

            Console.WriteLine($"Error in converting the Process status result: {response}");

            return null;
        }

        private Response<ProcessStatusResponse> ConvertToRunProcessResponse(string response)
        {
            try
            {
                var responseDes = JsonConvert.DeserializeObject<ProcessStatus>(response);
                var result = new ProcessStatusResponse()
                {
                    InstanceId = null,
                    State = responseDes.Status,
                    Status = ProcesioConverters.ConvertProcessStatus(responseDes.Status),
                    Variable = responseDes.Variable,
                };
                return new Response<ProcessStatusResponse>(result, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<LaunchResponse>(response);
                var result = new ProcessStatusResponse()
                {
                    InstanceId = responseDes.InstanceId,
                    State = 1,
                    Status = ProcesioConverters.ConvertProcessStatus(1),
                    Variable = null,
                };
                return new Response<ProcessStatusResponse>(result, new ErrorResponse());
            }
            catch { }

            try
            {
                var responseDes = JsonConvert.DeserializeObject<ErrorResponse>(response);
                return new Response<ProcessStatusResponse>(null, responseDes);
            }
            catch
            {
                Console.WriteLine("HTTP Response is not of type ErrorResponse.");
            }

            Console.WriteLine($"Error in converting the Process status result: {response}");

            return null;
        }




        private async Task<string> RunProcess(Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace, Uri runUri)
        {
            InitBasicClientHeaders(procesioToken, workspace);

            var payload = new
            {
                Payload = inputValues,
                ConnectionId = ""
            };
            var serializedInputValues = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(serializedInputValues, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(runUri, httpContent);
            var response = await httpResponse.Content.ReadAsStringAsync();
            return response;
        }

        private async Task<Dictionary<string, object>> ValidateInput(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken)
        {
            if (string.IsNullOrEmpty(processId))
            {
                throw new ArgumentNullException("Empty process instance id.");
            }
            await ValidateAuthentication(procesioToken);

            if (inputValues == null)
            {
                inputValues = new Dictionary<string, object>();
            }

            return inputValues;
        }

        #endregion
    }
}
