using Shared.Kernel.Util.Intergrity;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Messaging.Constanst.Contract
{
    /// <summary>
    /// Where extra 
    /// </summary>
    /// <typeparam name="PayloadType"></typeparam>
    public class BaseEvent<PayloadType>
    {
        // expect only payload can be change
        public required PayloadType Payload { get; set; }
       
        /// <summary>
        /// Check sum should bot be access externally
        /// </summary>
        private string Checksum { get; init; }
        public required DateTime Timestamp { get; init; }
        public required Guid EventId { get; init; } = Guid.NewGuid();
        public required string CorrelationId { get; init; }

        [SetsRequiredMembers]
        public BaseEvent(string correlationId, PayloadType payload)
        {
            EventId = Guid.NewGuid();
            CorrelationId = correlationId;
            Payload = payload;
            Timestamp = DateTime.UtcNow;
            Checksum = payload.ComputeChecksum(); // internal generate once
        }


        /// <summary>
        /// Hook for intergrity check
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return String.Compare(Payload.ComputeChecksum(), this.Checksum, StringComparison.Ordinal) == 0;
        }
    }
}
