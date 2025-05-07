using Shared.Infrastructure.Observability;

namespace Shared.Kernel.GeneralConfig
{
    public class ModuleMetaData
    {
        public IssuerType IssuerType { get; init; }
        public Guid IssuerId { get; init; }
    }
}
