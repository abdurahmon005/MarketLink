using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Notification;
using MarketLink.Domain.Enums;

namespace MarketLink.Application.Service
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationDto>> GetAsync(
            Guid userId, NotificationFilter filter, CancellationToken ct = default);

        Task MarkReadAsync(int notificationId, Guid userId, CancellationToken ct = default);
        Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);

        Task SendAsync(
            Guid userId, string title, string body,
            NotificationType type, int? orderId = null,
            CancellationToken ct = default);

        Task SaveDeviceTokenAsync(Guid userId, string token, string platform, CancellationToken ct = default);
        Task RemoveDeviceTokenAsync(Guid userId, string token, CancellationToken ct = default);
    }
}
