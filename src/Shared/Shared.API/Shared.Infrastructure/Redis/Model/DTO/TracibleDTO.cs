using Shared.Infrastructure.Observability;

namespace Shared.Infrastructure.Redis.Model.DTO
{
    public class ObservableDTO
    {
        public TraceId TraceId { get; set; }
        public long BatchId { get; set; }
    }
}
