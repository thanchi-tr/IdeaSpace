
namespace Shared.Domain.Models
{
    public class CursorPageResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public string? NextCursor { get; set; }  // null if no more pages
    }

}
