namespace Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = default!;
        public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
}
