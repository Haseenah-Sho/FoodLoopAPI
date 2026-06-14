using Application.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("8FD747F8-4405-4E39-8A89-766807C86A5A"),
                    Name = AppRoles.Admin,
                    CreatedBy = "admin@gmail.com",
                    DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new Role
                {
                    Id = Guid.Parse("5E4CEE2B-ADB0-43D1-94C6-F63E07553450"),
                    Name = AppRoles.Customer,
                    CreatedBy = "admin@gmail.com",
                    DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new Role
                {
                    Id = Guid.Parse("7C54F41F-A631-4394-A9D4-9BC56A404118"),
                    Name = AppRoles.Vendor,
                    CreatedBy = "admin@gmail.com",
                    DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                }
            );
        }
    }
}
