using Domain.Entities;

namespace Application.Repositories
{
    public interface IOrderListingRepository
    {
        Task AddAsync(OrderListing orderListing);
        Task<ICollection<OrderListing>> GetByOrderAsync(Guid orderId);
    }
}