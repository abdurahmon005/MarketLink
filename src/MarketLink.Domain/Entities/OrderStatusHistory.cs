using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Guid ChangedBy { get; set; }
        public string? Note { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
