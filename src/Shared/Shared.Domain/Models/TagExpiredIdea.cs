namespace Shared.Domain.Models
{
    public class TagExpiredIdea
    {
        public Guid TagId { get; set; }
        public Guid ExpiredIdeaId { get; set; }
        public Guid UserId { get; set; }    
        public virtual Tag Tag { get; set; }
        public virtual ExpiredIdea  ExpiredIdea { get; set; }
        public virtual User User { get; set; }
    }
}
