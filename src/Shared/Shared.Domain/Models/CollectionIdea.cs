namespace Shared.Domain.Models
{
    public class CollectionIdea
    {
        public Guid CollectionId { get; set; }
        public Guid IdeaId { get; set; }
        public virtual Collection Collection { get; set; }
        public virtual Idea Idea { get; set; }
    }
}
