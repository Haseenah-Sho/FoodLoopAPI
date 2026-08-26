namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string? FullName { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string HashPassword { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = false;
        public string? VerificationToken { get; set; } = default!;
        public DateTime? VerificationTokenExpiryTime { get; set; }
        public Customer? Customer { get; set; }
        public Vendor? Vendor { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public ICollection<Strike> Strikes { get; set; } = new HashSet<Strike>();
    }
}
