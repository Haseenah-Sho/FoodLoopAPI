using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderListingRepository(AppDbContext context) : IOrderListingRepository
    {
        public async Task AddAsync(OrderListing orderListing)
        {
            await context.Set<OrderListing>().AddAsync(orderListing);
        }

        public async Task<ICollection<OrderListing>> GetByOrderAsync(Guid orderId)
        {
            return await context.Set<OrderListing>()
                .Include(ol => ol.Listing)
                .Where(ol => ol.OrderId == orderId)
                .ToListAsync();
        }
    }
}