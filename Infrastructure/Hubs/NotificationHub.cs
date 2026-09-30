using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"user_{userId}");
        }

        public async Task JoinListingGroup(string listingId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"listing_{listingId}");
        }

        public async Task LeaveListingGroup(string listingId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"listing_{listingId}");
        }

        public async Task JoinBrowseGroup()
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, "browse_listings");
        }

        public async Task LeaveBrowseGroup()
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, "browse_listings");
        }
    }
}