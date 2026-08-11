namespace Domain.Entities
{
    public class Vendor : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string OrganizationName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsApproved { get; set; } = false;
        public ICollection<Listing> Listings { get; set; } = new HashSet<Listing>();

        // Payout details
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? PaystackSubaccountCode { get; set; }

        // Permanent registered location
        public string Address { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}