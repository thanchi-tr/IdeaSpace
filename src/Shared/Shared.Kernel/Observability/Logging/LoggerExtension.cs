using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Filters;

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
            
            var logPath = configuration["Logging:Path"] ?? "Logs";
            var rollingPeriod = Enum.TryParse<RollingInterval>(
                configuration["Logging:RollingOption"],
                ignoreCase: true,
                out var parsedRollingInterval
            ) ? parsedRollingInterval : RollingInterval.Day;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ServiceName", configuration["ServiceName"] ?? "Unknown")
                //.Enrich.WithMachineName() // these will be including the the appsetting of the API/WORKER of actual module
                //.Enrich.WithEnvironmentName()
                //.Enrich.WithThreadId()
                //.Enrich.WithProcessId()
                //.Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext() // traceId will be include in the LogContext 
                .WriteTo.Async(a => a.Console(outputTemplate: "[{Type} {TraceId} {Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}"))
                // Hard code to ensure all module write to the same pool (as oppose to individual decide where to log
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.WithProperty("Type", "Audit"))
                    .WriteTo.File($"{logPath}/Audit/audit-.log", rollingInterval: parsedRollingInterval)
                )
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.WithProperty("Type", "Recovery.Event"))
                    .WriteTo.File($"{logPath}/Recovery/detect-.log", rollingInterval: parsedRollingInterval)
                )
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.WithProperty("Type", "Module.Event"))
                    .WriteTo.File($"{logPath}/Module/mod-.log", rollingInterval: parsedRollingInterval)
                )
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.WithProperty("Type", "Systematic.Event"))
                    .WriteTo.File($"{logPath}/Systematic/sys-.log", rollingInterval: parsedRollingInterval)
                )
                .CreateLogger();
            Log.ForContext("Type", "Systematic.Event").Information("Major system event, e.g: hard recovery in action, module un-reachable, log rolling reach");
            Log.ForContext("Type", "Module.Event").Information("Major Module event, e.g: soft recovery in action, self healing attempt with result");
            Log.ForContext("Type", "Audit").Information("Escalate event, intended for human agent review : e.g: repeated module self healling failure.");
            Log.ForContext("Type", "Recovery.Event").Information("Data intergrity failure detection");

            // so that we can see the serilog issue
            Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));
 

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
