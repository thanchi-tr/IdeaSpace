namespace Shared.Messaging.RabbitMqSettings
{
    public class EventEnvelope<TPayload>
    {
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public TPayload Payload { get; set; }
        public string Checksum { get; set; }
        public string CorrelationId { get; set; }
        public string EventType { get; set; }

    }
}
