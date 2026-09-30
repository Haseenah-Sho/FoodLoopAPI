using Domain.Enums;

namespace Application.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task NotifyStockChange(Guid listingId, int remainingPortion, string status);
        Task NotifyNewListing(Guid listingId, string foodName, string vendorName, string? primaryImageUrl);
    }
}