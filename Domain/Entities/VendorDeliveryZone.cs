namespace Domain.Entities
{
    public class VendorDeliveryZone : BaseEntity
    {
        public Guid VendorId { get; set; }
        public Vendor Vendor { get; set; } = default!;
        public string ZoneName { get; set; } = default!;
        public decimal Fee { get; set; }
    }
}