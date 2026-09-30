using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
    {
        public void Configure(EntityTypeBuilder<ListingImage> builder)
        {
            builder.HasKey(li => li.Id);

            builder.Property(li => li.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(li => li.IsPrimary)
                .HasDefaultValue(false);

            builder.HasOne(li => li.Listing)
                .WithMany(l => l.ListingImages)
                .HasForeignKey(li => li.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}