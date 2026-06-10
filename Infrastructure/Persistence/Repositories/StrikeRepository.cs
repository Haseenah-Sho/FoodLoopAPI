using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class StrikeRepository(AppDbContext context) : IStrikeRepository
    {
        public async Task AddAsync(Strike strike)
        {
            await context.Set<Strike>().AddAsync(strike);
        }

        public async Task<ICollection<Strike>> GetByUserAsync(Guid userId)
        {
            return await context.Set<Strike>()
                .Include(s => s.Order)
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();
        }

        public async Task<int> GetStrikeCountAsync(Guid userId)
        {
            return await context.Set<Strike>()
                .CountAsync(s => s.UserId == userId && !s.IsDeleted);
        }
    }
}