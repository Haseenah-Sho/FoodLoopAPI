using Domain.Entities;

namespace Application.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        void Update(Order order);
        Task<Order?> GetOrderAsync(Guid id);
        Task<Order?> GetOrderByOrderNoAsync(string orderNo);
        Task<ICollection<Order>> GetOrdersByCustomerAsync(Guid customerId);
        Task<ICollection<Order>> GetOrdersByVendorAsync(Guid vendorId);
        Task<ICollection<Order>> GetAllOrdersAsync();
        Task<ICollection<Order>> GetStalePendingOrdersAsync(TimeSpan olderThan);

    }
}