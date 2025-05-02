using PersistGateKeeper.Infrastructure.Constant;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Infrastructure.Observability;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PersistGateKeeper.Infrastructure.Model.Recovery
{
    public class DeltaLogEvent
    {
        public long SequenceId { get; init; }
        public TraceId TraceId { get; init; }
        public DateTime Timestamp { get; init; }
        // After save, batch id might be out of sync
        public long BatchId { get; set; }
        private string _eventType;
        public string EventType { 
            get => _eventType;
            set => _eventType = (DomainEventTypes.All.Contains(value))
                ? value
                : DomainEventTypes.Empty;
        }

        public DLStatus Status { get; set; } = DLStatus.Queue;
        public string CheckSum { get; init; }

        public static string ComputeChecksum(DeltaLogEvent dlEvent)
        {
            // convert to stabel byte
            var json = JsonSerializer.Serialize(dlEvent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            });
            var bytes = Encoding.UTF8.GetBytes(json);

            using var sha = SHA512.Create();
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

    }
}
