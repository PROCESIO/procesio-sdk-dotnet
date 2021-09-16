using ProcesioSDK.Contracts;

namespace ProcesioSDK.Dto
{
    public class Response<T> : IResponse<T>
    {
        public bool IsFailure { get { return !IsSuccess; } }
        public bool IsSuccess { get { return Content != null; } }
        public T Content { get; set; }
        public IErrorResponse Error { get; set; }

        public Response(T content, IErrorResponse error)
        {
            Content = content;
            Error = error;
        }
    }
}
