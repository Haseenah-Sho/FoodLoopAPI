using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.MessageContent)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.ReferenceId)
                .IsRequired(false);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Property(n => n.NotificationType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);

            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}