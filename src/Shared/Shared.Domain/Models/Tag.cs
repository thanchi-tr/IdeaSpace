namespace Shared.Domain.Models
{
    public class Tag
    {
        public Guid TagId { get; init; } = Guid.NewGuid();
        public string Description { get; set; }

        public virtual ICollection<TagIdea> TagIdeas { get; set; }
        public virtual ICollection<TagExpiredIdea> TagExpiredIdeas { get; set; }
    }
}
