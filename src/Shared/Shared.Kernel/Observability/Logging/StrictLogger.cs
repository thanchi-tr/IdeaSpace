using Serilog;

namespace Shared.Kernel.Observability.Logging
{
    public static class StrictLogger
    {
        public static Dictionary<String, ILogger> Split(this ILogger logger)
        {

            return new Dictionary<String, ILogger>
            {
                { LoggerType.ModuleLog, logger.ForContext("Type", LoggerType.ModuleLog) },
                { LoggerType.SystematicLog, logger.ForContext("Type", LoggerType.SystematicLog) },
                { LoggerType.RecoveryLog, logger.ForContext("Type", LoggerType.RecoveryLog) },
                { LoggerType.AuditLog, logger.ForContext("Type", LoggerType.AuditLog) }
            };
        }
    }

}
