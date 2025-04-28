using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Models;

namespace Shared.Domain.ModelConfigurations
{
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.Property(l => l.LabelId)
                .ValueGeneratedNever()
                .IsRequired();
            builder.Property(l => l.Description)
                .HasColumnType("citext")
                .IsRequired(false);

            // relationship
            builder.HasMany(l => l.Collections)
                .WithOne(c => c.Label)
                .HasForeignKey(c => c.LabelId)
                .IsRequired(false);
        }
    }
}
