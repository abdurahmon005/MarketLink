using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationFilter
    {
        public bool? IsRead { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DeviceTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
}
