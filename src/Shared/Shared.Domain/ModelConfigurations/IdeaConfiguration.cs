using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
    {
        public void Configure(EntityTypeBuilder<Idea> builder)
        {
            builder.HasKey(i => i.IdeaId);

            builder.Property(i => i.IdeaId) // note that the Id won't be added until saveAsync is called
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(i => i.SerialisedQuestion)
                .HasColumnType("citext")
                .IsRequired();
            builder.Property(i => i.SerialisedSampleAnswer)
                   .HasColumnType("citext");
            builder.Property(i => i.Level)
                .HasDefaultValue(Level.Fresh)
                .IsRequired();
            builder.Property(i => i.QuestionType)
                .HasConversion<string>()
                .HasDefaultValue(QuestionType.ShortAnswer)
                .IsRequired();
            builder.Property(i => i.PrevRevisedResult)
                .HasDefaultValue(true);

            // relationship
            builder.HasOne(i => i.User)
                .WithMany(o => o.Ideas)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasMany(i => i.TagIdeas)
                .WithOne(ti => ti.Idea)
                .HasForeignKey(ti => ti.IdeaId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasMany(i => i.CollectionIdeas)
                .WithOne(ti => ti.Idea)
                .HasForeignKey(ti => ti.IdeaId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(true);
            
        }
    }
}
