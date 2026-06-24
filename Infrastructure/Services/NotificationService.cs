using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Hubs;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                MessageContent = message,
                NotificationType = type,
                IsRead = false,
                CreatedBy = "System"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.MessageContent,
                    type = notification.NotificationType.ToString(),
                    createdAt = notification.DateCreated
                });
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null) return;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyStockChange(Guid listingId, int remainingPortion, string status)
        {
            await _hubContext.Clients
                .Group($"listing_{listingId}")
                .SendAsync("ListingStockChanged", new
                {
                    listingId,
                    remainingPortion,
                    status
                });
        }

        public async Task NotifyNewListing(Guid listingId, string foodName, string vendorName, string? primaryImageUrl)
        {
            await _hubContext.Clients
                .Group("browse_listings")
                .SendAsync("NewListingPosted", new
                {
                    listingId,
                    foodName,
                    vendorName,
                    primaryImageUrl
                });
        }
    }
}