using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Interface.Extension.Operation
{
    public interface IExtractHashEntries
    {
        HashEntry[] ToHashEntries();
    }
}
