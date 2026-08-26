using Application.Repositories;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class ListingExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<ListingExpiryService> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireOverdueListingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while expiring overdue listings.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ExpireOverdueListingsAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();

            var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var overdueListings = await listingRepository.GetActiveListingsPastPickUpEndAsync();

            if (overdueListings.Count == 0) return;

            foreach (var listing in overdueListings)
            {
                listing.Status = ListingStatus.Expired;
                listingRepository.Update(listing);
            }

            await unitOfWork.SaveAsync();

            foreach (var listing in overdueListings)
            {
                await notificationService.NotifyStockChange(
                    listing.Id, listing.RemainingPortion, ListingStatus.Expired.ToString());
            }

            logger.LogInformation("Expired {Count} overdue listing(s).", overdueListings.Count);
        }
    }
}