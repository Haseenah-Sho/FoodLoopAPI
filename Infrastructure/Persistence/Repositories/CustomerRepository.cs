using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CustomerRepository(AppDbContext context) : ICustomerRepository
    {
        public async Task AddAsync(Customer customer)
        {
            await context.Set<Customer>().AddAsync(customer);
        }

        public void Update(Customer customer)
        {
            context.Set<Customer>().Update(customer);
        }

        public async Task<Customer?> GetCustomerAsync(Guid id)
        {
            return await context.Set<Customer>()
                .Include(c => c.User)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(Guid userId)
        {
            return await context.Set<Customer>()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
        }

        public async Task<ICollection<Customer>> GetAllCustomersAsync()
        {
            return await context.Set<Customer>()
                .Include(c => c.User)
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }
    }
}