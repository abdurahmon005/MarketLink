using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
