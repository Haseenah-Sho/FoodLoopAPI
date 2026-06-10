using Domain.Entities;

namespace Application.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByOrderAsync(Guid orderId);
        Task<Payment?> GetPaymentAsync(Guid id);
        void Update(Payment payment);
    }
}