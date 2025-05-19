using Shared.Domain.Constant.RabbitMQ.Type;

namespace Shared.Domain.Constanst.RabbitMQ.Event
{
    public class CacheEventPayload<ObjectT> 
    {
        public CacheEventType Type { get; set; }
        public ObjectT Data { get; set; }
    }
}
