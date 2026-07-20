using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class SupplierNotification
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public SupplierNotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public int? RelatedOrderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Company Company { get; set; } = null!;
        public Order? RelatedOrder { get; set; }
    }
}
