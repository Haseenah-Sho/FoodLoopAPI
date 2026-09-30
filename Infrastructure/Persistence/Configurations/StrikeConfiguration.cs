using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class StrikeConfiguration : IEntityTypeConfiguration<Strike>
    {
        public void Configure(EntityTypeBuilder<Strike> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ReasonForStrike)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasOne(s => s.User)
                .WithMany(u => u.Strikes)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}