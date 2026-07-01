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
    }
}
