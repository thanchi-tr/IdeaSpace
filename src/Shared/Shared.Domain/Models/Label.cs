namespace Shared.Domain.Models
{
    public class Label
    {
        public Guid LabelId { get; init; } = Guid.NewGuid();
        public string Description { get; set; }
        public virtual ICollection<Collection> Collections { get; set; }
    }
}
