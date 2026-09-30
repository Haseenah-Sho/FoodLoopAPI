using Domain.Entities;

namespace Application.Repositories
{
    public interface IRatingRepository
    {
        Task AddAsync(Rating rating);
        Task<ICollection<Rating>> GetByListingAsync(Guid listingId);
        Task<bool> HasCustomerRatedListingAsync(Guid customerId, Guid listingId);
    }
}