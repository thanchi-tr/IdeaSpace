
using System.Diagnostics.CodeAnalysis;

namespace Shared.Messaging.Constanst.Contract
{
    public class BaseEvent<EventType, EventChangeType>
    {
        public required Guid EventId { get; init; } = Guid.NewGuid();
        public required string CorrelationId { get; set; }
        public required EventChangeType ChangeType { get; set; }

        public required EventType Payload { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string Checksum { get; set; }

        [SetsRequiredMembers]
        public BaseEvent(string correlationId, EventType payload, EventChangeType changeType, string checksum)
        {
            EventId = Guid.NewGuid();
            CorrelationId = correlationId;
            Payload = payload;
            Timestamp = DateTime.UtcNow;
            Checksum = checksum;
            ChangeType = changeType;
        }
    }
}
