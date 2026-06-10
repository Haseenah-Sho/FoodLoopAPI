using Domain.Entities;

namespace Application.Repositories
{
    public interface IRatingRepository
    {
        Task AddAsync(Rating rating);
        Task<ICollection<Rating>> GetByListingAsync(Guid listingId);
        Task<ICollection<Rating>> GetByVendorAsync(Guid vendorId);
        Task<bool> HasCustomerRatedListingAsync(Guid customerId, Guid listingId);
        Task<bool> HasCustomerRatedVendorAsync(Guid customerId, Guid vendorId);
    }
}