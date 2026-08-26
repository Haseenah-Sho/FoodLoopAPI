namespace Domain.Entities
{
    public class Strike : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public string ReasonForStrike { get; set; } = default!;
    }
}
