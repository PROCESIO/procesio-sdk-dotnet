using ProcesioSDK.Responses;
using ProcesioSDK.Utilities;
using System.Threading.Tasks;

namespace ProcesioSDK
{
    public interface IProcesioClient
    {
        Task<ProcesioTokens> Authenticate(ProcesioUser procesioUser);
        Task<ProcesioTokens> RefreshToken(string refreshToken);

        Task<Flows> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> LaunchProjectInstance(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> RunProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> UploadFileFlow(UploadFileParam uploadFileParam, ProcesioTokens procesioTokens, string workspace);
        void GetFileIds(Flows flow);
    }
}
