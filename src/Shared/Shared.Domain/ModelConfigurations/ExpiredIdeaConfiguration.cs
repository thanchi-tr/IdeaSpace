using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class ExpiredIdeaConfiguration : IEntityTypeConfiguration<ExpiredIdea>
    {
        public void Configure(EntityTypeBuilder<ExpiredIdea> builder)
        {
            builder.Property(ei => ei.ExpiredIdeaId)
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

            // realtionship
            builder.HasOne(ei => ei.User)
                .WithMany(u => u.ExpiredIdeas)
                .HasForeignKey(ei => ei.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasMany(ei => ei.TagExpiredIdeas)
                .WithOne(tei => tei.ExpiredIdea)
                .HasForeignKey(tei => tei.ExpiredIdeaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(ei => ei.CollectionExpiredIdeas)
                .WithOne(cei => cei.ExpiredIdea)
                .HasForeignKey(cei => cei.ExpiredIdeaId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
