using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(d => d.Fee)
                .HasColumnType("decimal(18,2)");

            builder.Property(d => d.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(d => d.DispatchedAt)
                .IsRequired(false);

            builder.Property(d => d.DeliveredAt)
                .IsRequired(false);

        }
    }
}