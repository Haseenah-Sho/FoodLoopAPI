using Domain.Entities;

namespace Application.Repositories
{
    public interface IListingRepository
    {
        Task AddAsync(Listing listing);
        Task<Listing?> GetListingAsync(Guid id);
        Task<ICollection<Listing>> GetAllActiveListingsAsync();
        Task<ICollection<Listing>> GetListingsByVendorAsync(Guid vendorId);
        void Update(Listing listing);
    }
}