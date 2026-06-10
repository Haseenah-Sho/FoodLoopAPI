using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(User user)
        {
            await context.Set<User>().AddAsync(user);
        }

        public async Task<User?> GetAsync(Guid id)
        {
            return await context.Set<User>()
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User?> GetAsync(string email)
        {
            return await context.Set<User>()
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> IsExistAsync(string email)
        {
            return await context.Set<User>()
                .AnyAsync(x => x.Email == email && !x.IsDeleted);
        }

        public void Update(User user)
        {
            context.Set<User>().Update(user);
        }
    }
}
