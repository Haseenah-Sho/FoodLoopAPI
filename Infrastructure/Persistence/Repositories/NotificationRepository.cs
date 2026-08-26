using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository(AppDbContext context) : INotificationRepository
    {
        public async Task AddAsync(Notification notification)
        {
            await context.Set<Notification>().AddAsync(notification);
        }

        public void Update(Notification notification)
        {
            context.Set<Notification>().Update(notification);
        }

        public async Task<Notification?> GetNotificationAsync(Guid id)
        {
            return await context.Set<Notification>()
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        }

        public async Task<ICollection<Notification>> GetByUserAsync(Guid userId)
        {
            return await context.Set<Notification>()
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
        }
    }
}