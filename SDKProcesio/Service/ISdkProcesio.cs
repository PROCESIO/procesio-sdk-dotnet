using SDKProcesio.Responses;
using SDKProcesio.Utilities;
using System.Threading.Tasks;

namespace SDKProcesio.Service
{
    public interface ISdkProcesio
    {
        Task<Flows> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> LaunchProjectInstance(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> RunProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> UploadFileFlow(UploadFileParam uploadFileParam, ProcesioTokens procesioTokens, string workspace);
        void GetFileIds(Flows flow);
        Task<ProcesioTokens> Authenticate(ProcesioUser procesioUser);
        Task<ProcesioTokens> RefreshToken(string refreshToken);
    }
}
