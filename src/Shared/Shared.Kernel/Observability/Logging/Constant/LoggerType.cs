namespace Shared.Kernel.Observability.Logging.Constant
{
    public sealed record LoggerType(string Name)
    {
        public static readonly LoggerType SystematicLog = new("Systematic.Event");
        public static readonly LoggerType ModuleLog = new ("Module.Event");
        public static readonly LoggerType AuditLog = new("Audit");
        public static readonly LoggerType RecoveryLog = new("Recovery.Event");

        public override string ToString() => Name;
    }
}
