using Domain.Enums;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public decimal? Amount { get; set; }
        public PaystackStatus Status { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
