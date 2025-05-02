using System.Globalization;

namespace Shared.Infrastructure.Observability
{
    public class TraceId
    {
        public IssuerType IssuerType { get; init; }
        public Guid IssuerId { get; init; } 
        public DateTime Timestamp { get; init; }
        public override string ToString()
        {
            return $"{IssuerType.ToString()}:{IssuerId}:{Timestamp.ToUniversalTime():0}";
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

            res = new TraceId
            {
                IssuerType = issuerType,
                IssuerId = guid,
                Timestamp = timestamp
            };

            return true;
        }

    }
}
