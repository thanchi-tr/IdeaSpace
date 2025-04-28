using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class CollectionIdeaConfiguration : IEntityTypeConfiguration<CollectionIdea>
    {
        public void Configure(EntityTypeBuilder<CollectionIdea> builder)
        {
            builder.HasKey(ci => new { ci.CollectionId, ci.IdeaId });
            builder.HasOne(ci => ci.Collection)
                .WithMany(c => c.CollectionIdeas)
                .HasForeignKey(ci => ci.CollectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(ci => ci.Idea)
                .WithMany(i => i.CollectionIdeas)
                .HasForeignKey(ci => ci.IdeaId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
