namespace Shared.Domain.Models
{
    public class CollectionExpiredIdea
    {
        public Guid ExpiredIdeaId { get; set; }
        public Guid CollectionId { get; set; }

        public virtual ExpiredIdea ExpiredIdea { get; set; }
        public virtual Collection Collection {  get; set; }
    }
}
