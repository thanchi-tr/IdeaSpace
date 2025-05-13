using System.Net;

namespace APIGateway.API.Service.Dispatcher
{
    public sealed class DispatchResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public HttpStatusCode StatusCode { get; }
        public string? ErrorMessage { get; }
        public string? RawContent { get; }

        public DispatchResult(HttpStatusCode statusCode, T? data, string? errorMessage = null, string? rawContent = null)
        {
            StatusCode = statusCode;
            Data = data;
            ErrorMessage = errorMessage;
            RawContent = rawContent;
            IsSuccess = (int)statusCode is >= 200 and < 300;
        }

        public static DispatchResult<T> FromSuccess(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new(statusCode, data);

        public static DispatchResult<T> FromFailure(HttpStatusCode statusCode, string message, string? raw = null)
            => new(statusCode, default, message, raw);
    }
}
