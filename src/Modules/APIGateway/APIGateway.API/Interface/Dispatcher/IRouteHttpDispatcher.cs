using APIGateway.API.Service.Dispatcher;

namespace APIGateway.API.Interface.RouteDispatcher
{
    public interface IRouteHttpDispatcher
    {
        Task<DispatchResult<T>> DispatchAsync<T>(
         string serviceName,
         HttpMethod method,
         string path,
         object? body = null,
         IDictionary<string, string>? headers = null,
         CancellationToken ct = default);
    }

}
