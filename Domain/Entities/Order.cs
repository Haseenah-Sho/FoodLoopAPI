using Domain.Enums;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNo { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string DeliveryAddress { get; set; } = default!;
        public FulfilmentType FulfilmentType { get; set; }
        public string? DeliveryZoneName { get; set; }
        public Payment? Payment { get; set; }
        public Delivery? Delivery { get; set; }
        public ICollection<OrderListing> OrderListings { get; set; } = new HashSet<OrderListing>();
        public bool DescriptionMismatchFlagged { get; set; }
        public string? DescriptionMismatchNote { get; set; }
        public DateTime? DescriptionMismatchFlaggedAt { get; set; }
        public DateTime? DescriptionMismatchVendorNotifiedAt { get; set; }
    }
}