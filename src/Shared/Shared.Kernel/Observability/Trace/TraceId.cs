using System.Globalization;

namespace Shared.Infrastructure.Observability
{
    public class TraceId
    {
        public IssuerType IssuerType { get; init; }
        public Guid IssuerId { get; init; } 
        public DateTime Timestamp { get; private set; }


        public override string ToString()
                => $"{IssuerType}:{IssuerId}:{Timestamp:O}";


        public TraceId(Guid issuerId, int issuerType)
        {
            IssuerId = issuerId;

            if (!Enum.IsDefined(typeof(IssuerType), issuerType))
                throw new ArgumentOutOfRangeException(nameof(issuerType), "Invalid issuer type");

            IssuerType = (IssuerType)issuerType;
            Timestamp = DateTime.Now;
        }
        public TraceId(Guid issuerId, IssuerType issuerType)
        {
            IssuerId = issuerId;
            IssuerType = issuerType;
            Timestamp = DateTime.Now;
        }

        public TraceId(IssuerType issuerType, Guid issuerId, DateTime timestamp)
        {
            IssuerType = issuerType;
            IssuerId = issuerId;
            Timestamp = timestamp;
        }

        public void Refresh()
        {
            this.Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Attempt to deserialised the string back to readable
        /// </summary>
        /// <param name="traceStr"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool TryParse(string traceStr, out TraceId? res)
        {
            res = default;

            if (string.IsNullOrWhiteSpace(traceStr))
                return false;
            

            var parts = traceStr.Split(':', 3);
            if (parts.Length != 3)
                return false;

            var issuerPart = parts[0];
            var idPart = parts[1];
            var timePart = parts[2];

            if (!Enum.TryParse<IssuerType>(
                issuerPart, 
                ignoreCase: true,
                out var issuerType)
                )
                return false;

            if (!Guid.TryParse(
                idPart, 
                out var guid)
                )
                return false;
            if (!DateTime.TryParseExact(
                    parts[2],
                    "O",  // Round-trip ISO format
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var timestamp))
                return false;

            res = new TraceId( issuerType, guid, timestamp);
            return true;
        }

    }
}
