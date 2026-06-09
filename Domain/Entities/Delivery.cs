using Domain.Enums;

namespace Domain.Entities
{
    public class Delivery : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public string DeliveryAddress { get; set; } = default!;
        public decimal Fee { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
