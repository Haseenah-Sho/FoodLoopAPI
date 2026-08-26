using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class DeliveryRepository(AppDbContext context) : IDeliveryRepository
    {
        public async Task AddAsync(Delivery delivery)
        {
            await context.Set<Delivery>().AddAsync(delivery);
        }

        public void Update(Delivery delivery)
        {
            context.Set<Delivery>().Update(delivery);
        }

        public async Task<Delivery?> GetByOrderAsync(Guid orderId)
        {
            return await context.Set<Delivery>()
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.OrderId == orderId && !d.IsDeleted);
        }
    }
}