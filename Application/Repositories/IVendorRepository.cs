using Domain.Entities;

namespace Application.Repositories
{
    public interface IVendorRepository
    {
        Task AddAsync(Vendor vendor);
        Task<Vendor?> GetVendorAsync(Guid id);
        Task<Vendor?> GetVendorByUserIdAsync(Guid userId);
        Task<ICollection<Vendor>> GetAllVendorsAsync();
        Task<ICollection<Vendor>> GetPendingVendorsAsync();
        Task<bool> IsExistAsync(Guid userId);
        void Update(Vendor vendor);
    }
}