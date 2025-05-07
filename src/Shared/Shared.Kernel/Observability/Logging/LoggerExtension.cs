using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Shared.Kernel.Observability.Logging
{
    public static class LoggerExtension
    {
        /// <summary>
        /// Register Serilog adapter
        ///     - write to file in Logs/
        ///     - Write to console
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            // so that we can see the serilog issue
            Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Optional: register Serilog for dependency injection-based logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            return services;
        }
    }
}
