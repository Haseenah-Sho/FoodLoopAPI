using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class PaymentRepository(AppDbContext context) : IPaymentRepository
    {
        public async Task AddAsync(Payment payment)
        {
            await context.Set<Payment>().AddAsync(payment);
        }

        public void Update(Payment payment)
        {
            context.Set<Payment>().Update(payment);
        }

        public async Task<Payment?> GetPaymentAsync(Guid id)
        {
            return await context.Set<Payment>()
                .Include(p => p.Order)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Payment?> GetByOrderAsync(Guid orderId)
        {
            return await context.Set<Payment>()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.IsDeleted);
        }

        public async Task<Payment?> GetByReferenceAsync(string reference)
        {
            return await context.Set<Payment>()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaystackReference == reference && !p.IsDeleted);
        }

        public async Task<ICollection<Payment>> GetByUserAsync(Guid userId)
        {
            return await context.Set<Payment>()
                .Include(p => p.Order)
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Payment>> GetAllPaymentsAsync()
        {
            return await context.Set<Payment>()
                .ToListAsync();
        }
    }
}