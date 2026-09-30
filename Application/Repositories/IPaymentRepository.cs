using Domain.Entities;

namespace Application.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByOrderAsync(Guid orderId);
        Task<Payment?> GetPaymentAsync(Guid id);
        Task<Payment?> GetByReferenceAsync(string reference);
        Task<ICollection<Payment>> GetByUserAsync(Guid userId);
        void Update(Payment payment);
        Task<ICollection<Payment>> GetAllPaymentsAsync();
    }
}