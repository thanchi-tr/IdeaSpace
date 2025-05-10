using Serilog;
using Shared.Infrastructure.Observability;
using Shared.Kernel.Observability.Logging.Constant;

namespace Shared.Kernel.Observability.Logging
{
    public static class StrictLoggerExtension
    {
        public static Dictionary<LoggerType, ILogger> Split(this ILogger logger)
        {

            return new Dictionary<LoggerType, ILogger>
            {
                { LoggerType.ModuleLog, logger.InjectLoggerType( LoggerType.ModuleLog) },
                { LoggerType.SystematicLog, logger.InjectLoggerType( LoggerType.SystematicLog) },
                { LoggerType.RecoveryLog, logger.InjectLoggerType( LoggerType.RecoveryLog) },
                { LoggerType.AuditLog, logger.InjectLoggerType( LoggerType.AuditLog) }
            };
        }

        /// <summary>
        /// Enrich the logger with service type
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ServiceType"> Should be access via Constant AvailableService</param>
        /// <returns></returns>
        public static ILogger InjectServiceType(this ILogger logger, ServiceType serviceType)
        {
            if (serviceType is null)
                throw new ArgumentNullException(nameof(serviceType));

            return logger.ForContext("ServiceType", serviceType.Name);
        }

        /// <summary>
        /// Enrich the logger with service type
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ServiceType"> Should be access via Constant AvailableService</param>
        /// <returns></returns>
        public static ILogger InjectLoggerType(this ILogger logger, LoggerType loggerType)
        {
            if (loggerType is null)
                throw new ArgumentNullException(nameof(loggerType));

            return logger.ForContext("LogType", loggerType.Name);
        }


        
    }

}
