using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.UserId)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(u => u.UserName)
                .HasColumnType("citext");
            builder.Property(u => u.FirstName)
                .HasColumnType("citext");
            builder.Property(u => u.LastName)
                .HasColumnType("citext");
            builder.Property(u => u.NickName)
                .HasColumnType("citext");

            // relationship
            builder.HasMany(u => u.Ideas)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasMany(u => u.ExpiredIdeas)
                .WithOne(ei => ei.User)
                .HasForeignKey(ei => ei.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasMany(u => u.AccessCollections)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.OwnedCollections)
                .WithOne(c => c.Author)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.TagIdeas)
                .WithOne(ti => ti.User)
                .HasForeignKey(ti => ti.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasMany(u => u.TagExpiredIdeas)
                .WithOne(tei => tei.User)
                .HasForeignKey(tei => tei.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
