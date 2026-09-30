using Domain.Entities;

namespace Application.Repositories
{
    public interface IRoleRepository
    {
            Task AddAsync(Role role);
            Task<Role?> GetAsync(Guid id);
            Task<Role?> GetAsync(string name);
            Task<bool> IsExistAsync(string name);
            Task<ICollection<Role>> GetAllAsync();
    }
}
