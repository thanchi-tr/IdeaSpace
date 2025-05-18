using System.Linq.Expressions;

namespace Shared.Domain.Models
{
    public class CursorPageRequest<ORMType>
    {
        public string? AfterCursor { get; set; } // Base64 or delimited string
        public int Limit { get; set; } = 20;
        public Expression<Func<ORMType, bool>>? Filter { get; set; }
    }
}

