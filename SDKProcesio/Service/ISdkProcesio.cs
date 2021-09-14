using SDKProcesio.Utilities;
using System.Threading.Tasks;

namespace SDKProcesio.Service
{
    public interface ISdkProcesio
    {
        Task<string> PublishProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> LaunchProjectInstance(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<string> RunProject(string id, object requestBody, string workspace, ProcesioTokens procesioTokens);
        Task<ProcesioTokens> Authenticate(ProcesioUser procesioUser);
        Task<ProcesioTokens> RefreshToken(string refreshToken);
    }
}
