using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Constanst.Contract
{
    public class BaseEvent
    {
        public required Guid EventId { get; set; }
        public required string CollectionId { get; set; }
        public required string ChangeType { get; set; }

        public required string Payload { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string Checksum { get; set; }
    }
}
