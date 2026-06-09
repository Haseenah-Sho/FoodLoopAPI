using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.OrganizationName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(v => v.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.IsApproved)
                .HasDefaultValue(false);

            builder.HasOne(v => v.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.Listings)
                .WithOne(l => l.Vendor)
                .HasForeignKey(l => l.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(v => v.Ratings)
                .WithOne(r => r.Vendor)
                .HasForeignKey(r => r.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}