using Domain.Entities;

namespace Application.Repositories
{
    public interface IStrikeRepository
    {
        Task AddAsync(Strike strike);
        Task<ICollection<Strike>> GetByUserAsync(Guid userId);
        Task<int> GetStrikeCountAsync(Guid userId);
    }
}