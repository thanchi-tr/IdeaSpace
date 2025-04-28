using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class TagIdeaConfiguration : IEntityTypeConfiguration<TagIdea>
    {
        public void Configure(EntityTypeBuilder<TagIdea> builder)
        {
            builder.HasKey(ti => new {ti.IdeaId,ti.TagId, ti.UserId});

            //relationship
            builder.HasOne(ti => ti.Tag)
                .WithMany(t => t.TagIdeas)
                .HasForeignKey(ti => ti.TagId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(ti => ti.User)
                .WithMany(u => u.TagIdeas)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(ti => ti.Idea)
                .WithMany(i => i.TagIdeas)
                .HasForeignKey(ti => ti.IdeaId )
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
