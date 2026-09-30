using Application.Repositories;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class PendingOrderExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingOrderExpiryService> logger) : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireStalePendingOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while expiring stale pending orders.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ExpireStalePendingOrdersAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();

            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var staleOrders = await orderRepository.GetStalePendingOrdersAsync(PendingTimeout);

            if (staleOrders.Count == 0) return;

            foreach (var order in staleOrders)
            {
                order.Status = OrderStatus.Cancelled;
                order.DateModified = DateTime.UtcNow;
                orderRepository.Update(order);

                foreach (var item in order.OrderListings)
                {
                    await listingRepository.RestoreStockAsync(item.ListingId, item.Quantity);
                }
            }

            await unitOfWork.SaveAsync();

            foreach (var order in staleOrders)
            {
                await notificationService.SendNotificationAsync(
                    order.Customer.UserId,
                    "Order Expired",
                    $"Your order {order.OrderNo} was cancelled automatically because payment wasn't completed in time.",
                    NotificationType.OrderStatusChanged);
            }

            logger.LogInformation("Auto-cancelled {Count} stale pending order(s).", staleOrders.Count);
        }
    }
}