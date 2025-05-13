using APIGateway.API.Interface.RouteDispatcher;

namespace APIGateway.API.Service.Dispatcher
{
    public sealed class RouteHttpDispatcher : IRouteHttpDispatcher
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<RouteHttpDispatcher> _log;

        public RouteHttpDispatcher(IHttpClientFactory f, ILogger<RouteHttpDispatcher> log)
            => (_factory, _log) = (f, log);

        public async Task<DispatchResult<T>> DispatchAsync<T>(
            string svc, HttpMethod verb, string path,
            object? body = null, IDictionary<string, string>? hdrs = null,
            CancellationToken ct = default)
        {
            var client = _factory.CreateClient(svc);

            var uri = new Uri(client.BaseAddress!, path.TrimStart('/'));
            using var req = new HttpRequestMessage(verb, uri);

            if (body != null)
                req.Content = JsonContent.Create(body);
            if(hdrs != null) foreach (var kv in hdrs)
            {
                req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
            req.Headers.TryAddWithoutValidation("X-Caller", "ApiGateway");

            var res = await client.SendAsync(req, ct);
            var payload = res.IsSuccessStatusCode
                ? await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
                : default;

            return new DispatchResult<T>(res.StatusCode, payload);
        }
    }

}
