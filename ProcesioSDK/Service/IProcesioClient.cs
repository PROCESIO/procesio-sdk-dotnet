using ProcesioSDK.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcesioSDK
{
    public interface IProcesioClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="procesioUser"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">This exception is thrown if the Procesio Authentication request failed</exception>
        Task<ProcesioToken> Authenticate(ProcesioUser procesioUser);
        Task<bool> RefreshToken(ProcesioToken procesioToken);

        Task<ProcesioProject> PublishProject(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace = null);
        Task<string> LaunchProjectInstance(string id, Dictionary<string, object> inputValues, ProcesioToken procesioToken, string workspace);
        Task<string> RunProject(string id, Dictionary<string, object> inputValues, string workspace, ProcesioToken procesioToken);
        Task<string> UploadInputFileToProject(string id, ProcesioFileContent fileContent, ProcesioToken procesioToken, string workspace);
        void GetFileIds(ProcesioProject flow);
    }
}
