using Domain.Entities;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsExistAsync(string email);
        Task AddAsync(User user);
        Task<User?> GetAsync(Guid id);
        Task<User?> GetAsync(string email);
        void Update(User user);
    }
}