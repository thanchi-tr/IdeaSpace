using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class CollectionExpiredIdeaConfiguration : IEntityTypeConfiguration<CollectionExpiredIdea>
    {
        public void Configure(EntityTypeBuilder<CollectionExpiredIdea> builder)
        {
            builder.HasKey(ci => new { ci.CollectionId, ci.ExpiredIdeaId });
            builder.HasOne(ci => ci.Collection)
                .WithMany(c => c.CollectionExpiredIdeas)
                .HasForeignKey(ci => ci.CollectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(ci => ci.ExpiredIdea)
                .WithMany(i => i.CollectionExpiredIdeas)
                .HasForeignKey(ci => ci.ExpiredIdeaId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
