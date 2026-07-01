using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderRepository(AppDbContext context) : IOrderRepository
    {
        public async Task AddAsync(Order order)
        {
            await context.Set<Order>().AddAsync(order);
        }

        public void Update(Order order)
        {
            context.Set<Order>().Update(order);
        }

        public async Task<Order?> GetOrderAsync(Guid id)
        {
            return await context.Set<Order>()
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderListings)
                    .ThenInclude(ol => ol.Listing)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<Order?> GetOrderByOrderNoAsync(string orderNo)
        {
            return await context.Set<Order>()
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderListings)
                    .ThenInclude(ol => ol.Listing)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo && !o.IsDeleted);
        }

        public async Task<ICollection<Order>> GetOrdersByCustomerAsync(Guid customerId)
        {
            return await context.Set<Order>()
                .Include(o => o.OrderListings)
                    .ThenInclude(ol => ol.Listing)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByVendorAsync(Guid vendorId)
        {
            return await context.Set<Order>()
                .Include(o => o.Customer).ThenInclude(c => c.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderListings).ThenInclude(ol => ol.Listing)
                .Where(o => o.OrderListings.Any(ol => ol.Listing.VendorId == vendorId) && !o.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetAllOrdersAsync()
        {
            return await context.Set<Order>()
                .Include(o => o.Payment)
                .ToListAsync();
        }
    }
}