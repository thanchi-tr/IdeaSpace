namespace Shared.Domain.Models
{
    public class TagIdea
    {
        public Guid TagId { get; set; }
        public Guid IdeaId { get; set; }
        public Guid UserId { get; set; }

        public virtual Tag Tag { get; set; }
        public virtual User User { get; set; }
        public virtual Idea Idea { get; set; }
    }
}
