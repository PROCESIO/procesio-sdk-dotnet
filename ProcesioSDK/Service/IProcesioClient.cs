using ProcesioSDK.Dto;
using ProcesioSDK.Dto.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcesioSDK
{
    public interface IProcesioClient
    {
        /// <summary>
        /// Authenticate user by credentials 
        /// </summary>
        /// <param name="procesioUser">The Authentication requires user Procesio credentials, as username, password, realm,
        ///  client id and grant type.</param>
        /// <returns>Procesio Tokens, as acces token, refresh token and token valability.</returns>
        /// <exception cref="HttpRequestException">This exception is thrown if the Procesio Authentication request failed</exception>
        Task<ProcesioToken> Authenticate(ProcesioUser procesioUser);

        /// <summary>
        /// Refresh user token valability
        /// </summary>
        /// <param name="procesioToken">The Refresh Token requires acces token, refresh token and token valability.</param>
        /// <returns>True, if token was updated, otherwise false.</returns>
        Task<bool> RefreshToken(ProcesioToken procesioToken);

        /// <summary>
        /// Publish the process to new runtime instance
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="inputValues">The name of the variables used for flow and their values</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <returns>The generated process instance or error response</returns>
        Task<Response<ProcessInstance>> PublishProcess(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Launch process instance.
        /// </summary>
        /// <param name="processInstanceId">The flow id</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <returns>The launch process response or error response</returns>
        Task<Response<LaunchResponse>> LaunchProcessInstance(string processInstanceId, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Run process
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="inputValues">The name of the variables used for flow and their values</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification method</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <returns>The launch process response or error response</returns>
        Task<Response<LaunchResponse>> RunProcess(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Run process and wait for the process status. If timeout expires, then the process instance id will be saved.
        /// </summary>
        /// <param name="processId">The process id to execute</param>
        /// <param name="inputValues">The input values used by the process</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <param name="timeOut">number of seconds to wait for the process or return the instance id</param>
        /// <returns></returns>
        Task<Response<ProcessStatusResponse>> RunProcessSync(string processId, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null, int timeOut = 60);

        /// <summary>
        /// Get the process status after or during execution
        /// </summary>
        /// <param name="processInstanceId">Process instance id, returned by publish process method</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <returns>The process status response</returns>
        Task<Response<ProcessStatusResponse>> GetProcessStatus(string processInstanceId, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Upload each input file used for process instance if the process requires it
        /// </summary>
        /// <param name="processInstanceId">Process instance id, returned by publish process method</param>
        /// <param name="fileContent">The file details, as file path, variable name, lenght</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name. Can be null if working on the personal workspace.</param>
        /// <returns>File id</returns>
        Task<Response<UploadResponse>> UploadInputFileToProcessInstance(string processInstanceId, ProcesioFileContent fileContent, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Gets the file information required for the file upload method
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        IEnumerable<ProcesioFileContent> GetInputFileInfo(ProcessInstance process);
    }
}
