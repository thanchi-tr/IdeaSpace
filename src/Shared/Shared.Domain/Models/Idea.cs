namespace Shared.Domain.Models
{
    public class Idea
    { 
        public Guid IdeaId { get; init; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string SerialisedQuestion { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastReview {  get; set; } = DateTime.Now;
        public bool PrevRevisedResult { get; set; }
        public Level Level { get; set; }
        public string SerialisedSampleAnswer { get; set; }

        public QuestionType QuestionType { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<TagIdea> TagIdeas { get; set; }
        public virtual ICollection<CollectionIdea> CollectionIdeas { get; set; }
    }
}
