namespace Shared.Domain.Models
{
    public class Collection
    {
        public Guid UserId { get; set; }
        public Guid AuthorId {  get; set; }
        public Guid CollectionId { get; init; } = Guid.NewGuid();
        public Guid ParentCollectionId { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public Guid LabelId { get; set; }

        public virtual User User { get; set; }
        public virtual User Author { get; set; }
        public virtual ICollection<CollectionIdea> CollectionIdeas { get; set; }
        public virtual ICollection<CollectionExpiredIdea> CollectionExpiredIdeas { get; set; }
        public virtual Collection ParentCollection { get; set; }
        public virtual ICollection<Collection> ChildCollections { get; set; }
        public virtual Label Label { get; set; }
    }
}
