using Shared.Infrastructure.Redis.Config;
using System.Reflection;
using Polly;
using Polly.Extensions.Http;
namespace APIGateway.API.Config
{
    public static class HttpClientExtension
    {
        public static IServiceCollection ConfigHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            // parse the 
            var modules = configuration.GetSection("AppMetaData")
                                .Get<AppMetaData>() ?? throw new InvalidDataException($"Invalid Configuration @{Assembly.GetEntryAssembly().GetName().Name}") ;
            if(modules == null) 
                return services;

            foreach (var module in modules.ModulesMDatas.Keys)
            {
                var target = modules.ModulesMDatas[module] ;
                services.AddHttpClient(target.Url,
                    client =>
                    {
                        client.BaseAddress = new Uri(target.Url);
                        client.Timeout = TimeSpan.FromSeconds(target.HttpClientTimeOut);

                    })
                    .AddPolicyHandler(HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt)))
                    .AddPolicyHandler(HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            handledEventsAllowedBeforeBreaking: 5,
                            durationOfBreak: TimeSpan.FromSeconds(30)
                        ));
            }

            return services;
        }
    }
}
