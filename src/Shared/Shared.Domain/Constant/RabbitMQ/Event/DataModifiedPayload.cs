using Shared.Domain.Constant.RabbitMQ.Type;

namespace Shared.Messaging.Constanst.Event
{
    /// <summary>
    /// DTO allow for nullable field as oppose to ORM type
    /// </summary>
    /// <typeparam name="DTOType"></typeparam>
    public class DataModifiedPayload<DTOType>
    {
        public ModificationEventType Type { get; set; }
        public DTOType Data { get; set; }
    }
}
