using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Stars)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        }
    }
}
