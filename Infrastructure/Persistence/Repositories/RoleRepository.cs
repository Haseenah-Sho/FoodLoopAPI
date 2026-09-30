using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepository(AppDbContext context) : IRoleRepository
    {
        public async Task AddAsync(Role role)
        {
            await context.Set<Role>().AddAsync(role);
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            return await context.Set<Role>()
                .Include(r => r.UserRoles)
                .Where(r => !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<Role?> GetAsync(Guid id)
        {
            return await context.Set<Role>()
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Role?> GetAsync(string name)
        {
            return await context.Set<Role>()
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync (r => r.Name == name && !r.IsDeleted);
        }

        public async Task<bool> IsExistAsync(string name)
        {
            return await context.Set<Role>()
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }
    }
}
