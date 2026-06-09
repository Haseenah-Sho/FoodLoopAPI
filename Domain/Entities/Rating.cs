namespace Domain.Entities
{
    public class Rating : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public Guid? VendorId { get; set; }
        public Vendor? Vendor { get; set; } = default!;
        public Guid? ListingId { get; set; }
        public Listing? Listing { get; set; } = default!;
        public int Stars {  get; set; }
        public string? Comment { get; set; } = default!;
    }
}
