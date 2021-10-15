using System.Collections.Generic;

namespace ProcesioSDK.Contracts
{
    public interface IResponse<T>
    {
        bool IsFailure { get; }
        bool IsSuccess { get; }
        T Content { get; set; }
        IEnumerable<IErrorResponse> Errors { get; set; }
    }
}
