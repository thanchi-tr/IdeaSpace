namespace Shared.Kernel.Interface.Health
{
    public interface IRecoveryHandler
    {
        bool IsOutage { get; }
        void OutageResovle();
    }
}
