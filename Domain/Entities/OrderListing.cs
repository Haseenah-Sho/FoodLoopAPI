namespace Domain.Entities
{
    public class OrderListing
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public Guid ListingId { get; set; }
        public Listing Listing { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
