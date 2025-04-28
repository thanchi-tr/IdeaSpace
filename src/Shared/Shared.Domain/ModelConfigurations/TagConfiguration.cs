using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.Property(t => t.TagId)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("citext")
                .IsRequired(false);


            // relationship
            builder.HasMany(t => t.TagIdeas)
                .WithOne(ti => ti.Tag)
                .HasForeignKey(ti => ti.TagId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(t => t.TagExpiredIdeas)
                .WithOne(tei => tei.Tag)
                .HasForeignKey(tei => tei.TagId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
