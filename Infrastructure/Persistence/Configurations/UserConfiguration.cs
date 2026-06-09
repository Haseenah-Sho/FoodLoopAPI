using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Property(u => u.UserName)
                .HasMaxLength(100);

            builder.Property(u => u.HashPassword)
                .IsRequired();

            builder.Property(u => u.VerificationToken)
                .HasMaxLength(4);

            builder.Property(u => u.VerificationTokenExpiryTime)
                .IsRequired(false);

            builder.Property(u => u.IsEmailVerified)
                .HasDefaultValue(false);

            builder.HasData(new User
            {
                Id = Guid.Parse("E01837EE-E5C5-48DB-98C2-CDC266139856"),
                FullName = "Admin",
                Email = "admin@gmail.com",
                HashPassword = "",
                IsEmailVerified = true,
                CreatedBy = "admin@gmail.com",
                DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
