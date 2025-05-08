using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using StackExchange.Redis;
namespace APIGateway.Infrastructure.Model.DTO
{
    public class RediRateBucket : IExtractHashEntries
    {
        
        public int RemainCount { get; set; }
        public long LastUpdatedEpoch { get; set; }
        
        public RediRateBucket(HashEntry[] entries)
        {
            var dict = entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
            RemainCount = int.Parse(dict["Remaining"]);
            LastUpdatedEpoch = long.Parse(dict["LastUpdatedEpoch"]);
        }

        public HashEntry[] ToHashEntries() =>
            new[]
            {
                new HashEntry("RemainCount", this.RemainCount),
                new HashEntry("LastUpdatedEpoch", this.LastUpdatedEpoch)
            };
    }
}
