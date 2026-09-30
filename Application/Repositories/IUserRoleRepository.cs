using Domain.Entities;

namespace Application.Repositories
{
    public interface IUserRoleRepository
    {
        Task AddAsync(UserRole userRole);
        Task<bool> IsExistAsync(Guid userId, Guid roleId);
    }
}
