using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderListingConfiguration : IEntityTypeConfiguration<OrderListing>
    {
        public void Configure(EntityTypeBuilder<OrderListing> builder)
        {
            builder.HasKey(ol => new { ol.OrderId, ol.ListingId });

            builder.Property(ol => ol.Quantity)
                .IsRequired();

            builder.HasOne(ol => ol.Order)
                .WithMany(o => o.OrderListings)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ol => ol.Listing)
                .WithMany()
                .HasForeignKey(ol => ol.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}