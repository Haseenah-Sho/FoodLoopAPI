using Domain.Enums;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string MessageContent { get; set; } = default!;
        public Guid? ReferenceId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
