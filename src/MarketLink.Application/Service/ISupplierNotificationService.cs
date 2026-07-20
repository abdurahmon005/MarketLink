using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.Domain.Enums;

namespace MarketLink.Application.Service
{
    public interface ISupplierNotificationService
    {
        Task<PagedResult<SupplierNotificationDto>> GetAsync(
            int companyId, NotificationFilter filter, CancellationToken ct = default);

        Task<int> GetUnreadCountAsync(int companyId, CancellationToken ct = default);

        Task MarkReadAsync(int notificationId, int companyId, CancellationToken ct = default);

        Task MarkAllReadAsync(int companyId, CancellationToken ct = default);

        Task SendAsync(
            int companyId,
            string title,
            string body,
            SupplierNotificationType type,
            int? relatedOrderId = null,
            CancellationToken ct = default);

        Task CheckAndSendLowStockAlertsAsync(int companyId, CancellationToken ct = default);
    }
}
