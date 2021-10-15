using ProcesioSDK.Contracts;
using System.Collections.Generic;

namespace ProcesioSDK.Dto
{
    public class Response<T> : IResponse<T>
    {
        public bool IsFailure { get { return !IsSuccess; } }
        public bool IsSuccess { get { return Content != null; } }
        public T Content { get; set; }
        public IEnumerable<IErrorResponse> Errors { get; set; }

        public Response(T content, IErrorResponse error)
        {
            Content = content;
            var errorList = new List<IErrorResponse>();
            errorList.Add(error);
            Errors = errorList;
        }

        public Response(T content, IEnumerable<IErrorResponse> errors)
        {
            Content = content;
            Errors = errors;
        }
    }
}
