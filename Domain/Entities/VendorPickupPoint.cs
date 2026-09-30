namespace Domain.Entities
{
    public class VendorPickupPoint : BaseEntity
    {
        public Guid VendorId { get; set; }
        public Vendor Vendor { get; set; } = default!;
        public string PointName { get; set; } = default!;
        public string Address { get; set; } = default!;
    }
}