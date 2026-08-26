using Domain.Entities;

namespace Application.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task<Customer?> GetCustomerAsync(Guid id);
        Task<Customer?> GetCustomerByUserIdAsync(Guid userId);
        Task<ICollection<Customer>> GetAllCustomersAsync();
        void Update(Customer customer);
    }
}