using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository(AppDbContext context) : IUserRoleRepository
    {
        public async Task AddAsync(UserRole userRole)
        {
            await context.Set<UserRole>().AddAsync(userRole);
        }

        public async Task<bool> IsExistAsync(Guid userId, Guid roleId)
        {
            return await context.Set<UserRole>()
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}
