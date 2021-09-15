using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDKProcesio.Config;
using SDKProcesio.Responses;
using SDKProcesio.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SDKProcesio.Service
{
    public class SdkProcesio : ISdkProcesio
    {
        private readonly HttpClient client;

        public Guid FlowId { get; set; }
        public Dictionary<string, Guid> FileMap { get; set; }
        public Dictionary<Guid,string> VariableNameMap { get; set; }

        public SdkProcesio()
        {
            client = new HttpClient();
        }

        public async Task<Flows> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens)
        {
            if (id == null || requestBody == null || procesioTokens.AccessToken == null || procesioTokens.RefreshToken == null)
            {
                return null;
            }

            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new Uri(Constants.ProcesioURL);
            Uri uri = new Uri(baseUri, string.Format(Constants.ProcesioPublishMethod, id));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("workspace",workspace);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            var httpContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync(uri, httpContent);

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

            Uri baseUri = new Uri(Constants.ProcesioURL);
            Uri uri = new Uri(baseUri, string.Format(Constants.ProcesioLaunchMethod, id));

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
            if (id == null || requestBody == null || procesioTokens.AccessToken == null || procesioTokens.RefreshToken == null)
            {
                return null;
            }
           
            if (procesioTokens.Expires_in <= 0)
            {
                await RefreshToken(procesioTokens.RefreshToken);
            }

            Uri baseUri = new Uri(Constants.ProcesioURL);
            Uri uri = new Uri(baseUri, string.Format(Constants.ProcesioRunMethod, id));

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

            Uri baseUri = new Uri(Constants.ProcesioURL);
            Uri uri = new Uri(baseUri, Constants.ProcesioUploadFlowFile);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("fileId", uploadFileParam.FileId.ToString());
            client.DefaultRequestHeaders.Add("flowInstanceId", uploadFileParam.FlowInstanceID.ToString());
            client.DefaultRequestHeaders.Add("variableName", uploadFileParam.VariableName);
            client.DefaultRequestHeaders.Add("workspace", workspace);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", procesioTokens.AccessToken);

            MultipartFormDataContent form = new MultipartFormDataContent();
            using (var memoryStream = new MemoryStream())
            {
                uploadFileParam.FileContent.CopyTo(memoryStream);
                var fileByte =  memoryStream.ToArray();
                form.Add(new ByteArrayContent(fileByte, 0, fileByte.Length), uploadFileParam.FileName, uploadFileParam.FileName);
            }
            form.Add(new StringContent(uploadFileParam.FileName), nameof(uploadFileParam.FileName));
            form.Add(new StringContent(uploadFileParam.Length), nameof(uploadFileParam.Length));

            var httpResponse = await client.PostAsync(uri, form);
            
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

            Uri baseUri = new Uri(Constants.ProcesioAuthURL);
            Uri uri = new Uri(baseUri, Constants.ProcesioAuthMethod);

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
            if(refreshToken == null)
            {
                return null;
            }

            var queryString = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            Uri baseUri = new Uri(Constants.ProcesioAuthURL);
            Uri uri = new Uri(baseUri, Constants.ProcesioAuthMethod);

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
