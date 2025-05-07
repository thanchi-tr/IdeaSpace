
namespace Shared.Kernel.Observability.Logging
{
    public static class LoggerType
    {
        //"Major system event, e.g: hard recovery in action, module un-reachable, log rolling reach"
        public const string SystematicLog = "Systematic.Event";

        //Major Module event, e.g: soft recovery in action, self healing attempt with result
        public const string ModuleLog = "Module.Event";

        //Escalate event, intended for human agent review : e.g: repeated module self healling failure.
        public const string AuditLog = "Audit";

        //Data intergrity failure detection
        public const string RecoveryLog = "Recovery.Event";
    }
}
