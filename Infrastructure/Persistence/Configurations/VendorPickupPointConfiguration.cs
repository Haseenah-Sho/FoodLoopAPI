using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VendorPickupPointConfiguration : IEntityTypeConfiguration<VendorPickupPoint>
    {
        public void Configure(EntityTypeBuilder<VendorPickupPoint> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PointName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Address)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasOne(p => p.Vendor)
                .WithMany(v => v.PickupPoints)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}