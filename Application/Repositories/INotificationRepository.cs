using Domain.Entities;

namespace Application.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<ICollection<Notification>> GetByUserAsync(Guid userId);
        Task<Notification?> GetNotificationAsync(Guid id);
        void Update(Notification notification);
    }
}