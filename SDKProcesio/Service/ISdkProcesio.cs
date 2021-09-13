using SDKProcesio.Utilities;
using System.Threading.Tasks;

namespace SDKProcesio.Service
{
    interface ISdkProcesio
    {
        Task<string> PublishProject(string projectId, string requestBody, string token);
        Task<string> LaunchFlowInstance(string flowId, string requestBody, string token);
        Task<string> RunProject(string id, string requestBody, string token);
        Task<ProcesioTokens> Authenticate(string realm, string grantType, string userName, string passw, string clientId);
        Task<ProcesioTokens> RefreshToken(string clientId, string refreshToken);
        Task<ProcesioTokens> GetProcesioTokens();
    }
}
