namespace ProcesioSDK.Contracts
{
    public interface IResponse<T>
    {
        bool IsFailure { get; }
        bool IsSuccess { get; }
        T Content { get; set; }
        IErrorResponse Error { get; set; }
    }
}
