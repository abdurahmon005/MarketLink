using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Notification;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<PagedResult<NotificationDto>> GetAsync(
            Guid userId, NotificationFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 100) filter.PageSize = 20;

            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(n => new NotificationDto
                {
                    Id        = n.Id,
                    Title     = n.Title,
                    Body      = n.Body,
                    Type      = n.Type,
                    IsRead    = n.IsRead,
                    OrderId   = n.OrderId,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<NotificationDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task MarkReadAsync(int notificationId, Guid userId, CancellationToken ct = default)
        {
            var n = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);

            if (n != null && !n.IsRead)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        }

        public async Task SendAsync(
            Guid userId, string title, string body,
            NotificationType type, int? orderId = null,
            CancellationToken ct = default)
        {
            _context.Notifications.Add(new Notification
            {
                UserId    = userId,
                Title     = title,
                Body      = body,
                Type      = type,
                IsRead    = false,
                OrderId   = orderId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Notification sent to user {UserId}: {Title}", userId, title);
        }

        public async Task SaveDeviceTokenAsync(
            Guid userId, string token, string platform, CancellationToken ct = default)
        {
            var platformEnum = Enum.TryParse<Domain.Enums.Platform>(platform, true, out var p)
                ? p
                : Domain.Enums.Platform.Web;

            var exists = await _context.DeviceTokens
                .AnyAsync(d => d.Token == token, ct);

            if (!exists)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId    = userId,
                    Token     = token,
                    Platform  = platformEnum,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveDeviceTokenAsync(Guid userId, string token, CancellationToken ct = default)
        {
            await _context.DeviceTokens
                .Where(d => d.UserId == userId && d.Token == token)
                .ExecuteDeleteAsync(ct);
        }
    }
}
