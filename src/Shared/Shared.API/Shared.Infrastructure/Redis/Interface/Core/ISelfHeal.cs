namespace Shared.Infrastructure.Redis.Interface.Core
{
    public interface ISelfHeal
    {
        bool IsConnectionHealthy();
        void AttemptHeal();
    }
}
