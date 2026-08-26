using Domain.Entities;

namespace Application.Repositories
{
    public interface IDeliveryRepository
    {
        Task AddAsync(Delivery delivery);
        Task<Delivery?> GetByOrderAsync(Guid orderId);
        void Update(Delivery delivery);
    }
}