using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.Property(c => c.CollectionId)
                .ValueGeneratedNever()
                .IsRequired();
            builder.Property(c => c.Description)
                .HasColumnType("citext")
                .IsRequired();

            // relationship
            builder.HasOne(c => c.User)
                .WithMany(u => u.AccessCollections)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(c => c.Author)
                .WithMany(u => u.OwnedCollections)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(c => c.ParentCollection)
                .WithMany(c => c.ChildCollections)
                .HasForeignKey(cc => cc.ParentCollectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(c => c.Label)
                .WithMany(l => l.Collections)
                .HasForeignKey(c => c.LabelId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasMany(c=>c.CollectionIdeas)
                .WithOne(ci => ci.Collection)
                .HasForeignKey(ci => ci.CollectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
