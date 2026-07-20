using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class SupplierNotificationService : ISupplierNotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SupplierNotificationService> _logger;

        public SupplierNotificationService(
            AppDbContext context,
            ILogger<SupplierNotificationService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<PagedResult<SupplierNotificationDto>> GetAsync(
            int companyId, NotificationFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 100) filter.PageSize = 20;

            var query = _context.SupplierNotifications
                .AsNoTracking()
                .Where(n => n.CompanyId == companyId);

            if (filter.UnreadOnly == true)
                query = query.Where(n => !n.IsRead);

            var total = await query.CountAsync(ct);

            var rows = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(n => new
                {
                    n.Id, n.Title, n.Body, n.Type, n.IsRead, n.RelatedOrderId, n.CreatedAt
                })
                .ToListAsync(ct);

            var items = rows.Select(n => new SupplierNotificationDto
            {
                Id             = n.Id,
                Title          = n.Title,
                Body           = n.Body,
                Type           = n.Type,
                IsRead         = n.IsRead,
                RelatedOrderId = n.RelatedOrderId,
                CreatedAt      = n.CreatedAt,
                TimeAgo        = CalcTimeAgo(n.CreatedAt)
            }).ToList();

            return new PagedResult<SupplierNotificationDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(int companyId, CancellationToken ct = default)
            => await _context.SupplierNotifications
                .CountAsync(n => n.CompanyId == companyId && !n.IsRead, ct);

        public async Task MarkReadAsync(int notificationId, int companyId, CancellationToken ct = default)
        {
            var notification = await _context.SupplierNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.CompanyId == companyId, ct);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task MarkAllReadAsync(int companyId, CancellationToken ct = default)
        {
            var unread = await _context.SupplierNotifications
                .Where(n => n.CompanyId == companyId && !n.IsRead)
                .ToListAsync(ct);

            foreach (var n in unread)
                n.IsRead = true;

            if (unread.Count > 0)
                await _context.SaveChangesAsync(ct);
        }

        public async Task SendAsync(
            int companyId,
            string title,
            string body,
            SupplierNotificationType type,
            int? relatedOrderId = null,
            CancellationToken ct = default)
        {
            _context.SupplierNotifications.Add(new SupplierNotification
            {
                CompanyId      = companyId,
                Title          = title,
                Body           = body,
                Type           = type,
                IsRead         = false,
                RelatedOrderId = relatedOrderId,
                CreatedAt      = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "SupplierNotification sent: CompanyId={CompanyId}, Type={Type}, Title={Title}",
                companyId, type, title);
        }

        public async Task CheckAndSendLowStockAlertsAsync(int companyId, CancellationToken ct = default)
        {
            const int threshold = 10;

            var lowStock = await _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId
                         && p.IsActive
                         && p.StockQuantity < threshold)
                .Select(p => new { p.Id, p.Name, p.StockQuantity })
                .ToListAsync(ct);

            foreach (var product in lowStock)
            {
                await SendAsync(
                    companyId,
                    title: "Kam qoldiq ogohlantirishi",
                    body:  $"'{product.Name}' mahsulotida faqat {product.StockQuantity} ta qoldiq qoldi.",
                    type:  SupplierNotificationType.LowStock,
                    ct: ct);
            }
        }

        private static string CalcTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;
            if (diff.TotalMinutes < 1)  return "Hozirgina";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min oldin";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} soat oldin";
            if (diff.TotalDays    < 30) return $"{(int)diff.TotalDays} kun oldin";
            return $"{(int)(diff.TotalDays / 30)} oy oldin";
        }
    }
}
