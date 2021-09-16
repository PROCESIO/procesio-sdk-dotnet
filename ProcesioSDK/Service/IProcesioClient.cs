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
        /// Publish the project to new runtime instance
        /// </summary>
        /// <param name="id">The project id</param>
        /// <param name="inputValues">The name of the variables used for flow and their values</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name</param>
        /// <returns>The variables, flow id, description, title, first name, last name, workspace name of the project</returns>
        Task<ProcesioProject> PublishProject(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null);

        /// <summary>
        /// Launch flow instance.
        /// </summary>
        /// <param name="id">The flow id</param>
        /// <param name="inputValues"></param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name</param>
        /// <returns>Instance id</returns>
        Task<string> LaunchProjectInstance(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace);

        /// <summary>
        /// Run project
        /// </summary>
        /// <param name="id">The project id</param>
        /// <param name="inputValues">The name of the variables used for flow and their values</param>
        /// <param name="workspace">The workspace name</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification method</param>
        /// <returns>Instance id</returns>
        Task<string> RunProject(string id, Dictionary<string, object> inputValues, string workspace, ProcesioToken procesioToken);

        /// <summary>
        /// Upload file used for flows
        /// </summary>
        /// <param name="id">Flow id, returned by PublishProject method</param>
        /// <param name="fileContent">The file details, as file path, variable name, lenght</param>
        /// <param name="procesioToken">The access token, refresh token and token valability, returned by Authentification</param>
        /// <param name="workspace">The workspace name</param>
        /// <returns>File id</returns>
        Task<string> UploadInputFileToProject(string id, ProcesioFileContent fileContent, ProcesioToken procesioToken, string workspace);

        /// <summary>
        /// Gets the file id needed for UploadInputFileToProject method
        /// </summary>
        /// <param name="flow"></param>
        void GetFileIds(ProcesioProject flow);
    }
}
