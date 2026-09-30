using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VendorDeliveryZoneConfiguration : IEntityTypeConfiguration<VendorDeliveryZone>
    {
        public void Configure(EntityTypeBuilder<VendorDeliveryZone> builder)
        {
            builder.HasKey(z => z.Id);

            builder.Property(z => z.ZoneName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(z => z.Fee)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasOne(z => z.Vendor)
                .WithMany(v => v.DeliveryZones)
                .HasForeignKey(z => z.VendorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}