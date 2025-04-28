using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class TagExpiredIdeaConfiguration : IEntityTypeConfiguration<TagExpiredIdea>
    {
        public void Configure(EntityTypeBuilder<TagExpiredIdea> builder)
        {
            builder.HasKey(tei => new { tei.TagId, tei.ExpiredIdeaId, tei.UserId });

            // relationship
            builder.HasOne(tei => tei.Tag)
                .WithMany(t => t.TagExpiredIdeas)
                .HasForeignKey(t => t.TagId)
                .IsRequired();

            builder.HasOne(tei => tei.ExpiredIdea)
                .WithMany(t => t.TagExpiredIdeas)
                .HasForeignKey(t => t.ExpiredIdeaId)
                .IsRequired();

            builder.HasOne(tei => tei.User)
                .WithMany(t => t.TagExpiredIdeas)
                .HasForeignKey(t => t.UserId)
                .IsRequired();
        }
    }
}
