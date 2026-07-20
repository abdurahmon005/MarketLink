using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Supplier
{
    public class SupplierNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public SupplierNotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public int? RelatedOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    public class NotificationFilter
    {
        public bool? UnreadOnly { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
