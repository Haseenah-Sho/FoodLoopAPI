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

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Property(u => u.HashPassword)
                .IsRequired();

            builder.Property(u => u.VerificationToken)
                .HasMaxLength(6);

            builder.Property(u => u.VerificationTokenExpiryTime)
                .IsRequired(false);

            builder.Property(u => u.IsEmailVerified)
                .HasDefaultValue(false);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasData(new User
            {
                Id = Guid.Parse("E01837EE-E5C5-48DB-98C2-CDC266139856"),
                FullName = "Admin",
                UserName = "admin",
                Email = "admin@gmail.com",
                HashPassword = "AQAAAAIAAYagAAAAEIlcXEQY7EF9J8g4o8G+1C8GSqLEQ4pZCfp5j5ygZ3kJc73c3EiCdl4C5baLWEp00Q==",
                IsEmailVerified = true,
                CreatedBy = "admin@gmail.com",
                DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
