using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.FoodName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.FoodDescription)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(l => l.Quantity)
                .IsRequired();

            builder.Property(l => l.QuantityPerUnit)
                .IsRequired();

            builder.Property(l => l.RemainingPortion)
                .IsRequired();

            builder.Property(l => l.IsFree)
                .HasDefaultValue(false);

            builder.Property(l => l.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(l => l.DeliveryFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(l => l.PickUpAvailable)
                .HasDefaultValue(false);

            builder.Property(l => l.DeliveryAvailable)
                .HasDefaultValue(false);

            builder.Property(l => l.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasMany(l => l.Ratings)
                .WithOne(r => r.Listing)
                .HasForeignKey(r => r.ListingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
