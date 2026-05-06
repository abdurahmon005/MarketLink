using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class CompanyOrderService : ICompanyOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyOrderService> _logger;

        // Ruxsat etilgan status o'tishlari
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            [OrderStatus.Pending]   = [OrderStatus.Accepted, OrderStatus.Rejected, OrderStatus.Cancelled],
            [OrderStatus.Accepted]  = [OrderStatus.Preparing, OrderStatus.Cancelled],
            [OrderStatus.Preparing] = [OrderStatus.Delivered],
        };

        // Stock qaytariladigan statuslar
        private static readonly HashSet<OrderStatus> StockRestoreStatuses =
            [OrderStatus.Cancelled, OrderStatus.Rejected];

        public CompanyOrderService(AppDbContext context, ILogger<CompanyOrderService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<PagedResult<OrderResponse>> GetIncomingOrdersAsync(
            int companyId, IncomingOrderFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 50) filter.PageSize = 10;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value.ToUniversalTime());

            if (filter.DateTo.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.DateTo.Value.ToUniversalTime());

            if (!string.IsNullOrWhiteSpace(filter.ShopName))
                query = query.Where(o => o.Shop.ShopName.Contains(filter.ShopName));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderResponse
                {
                    Id          = o.Id,
                    ShopId      = o.ShopId,
                    ShopName    = o.Shop.ShopName,
                    CompanyId   = o.CompanyId,
                    Status      = o.Status,
                    TotalAmount = o.TotalAmount,
                    Note        = o.Note,
                    CreatedAt   = o.CreatedAt,
                    UpdatedAt   = o.UpdatedAt,
                    Items       = o.Items.Select(oi => new OrderItemResponse
                    {
                        ProductId       = oi.ProductId,
                        ProductName     = oi.Product.Name,
                        ProductImageUrl = oi.Product.ImageUrl,
                        Quantity        = oi.Quantity,
                        UnitPrice       = oi.UnitPrice,
                        Subtotal        = oi.Quantity * oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<OrderResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(
            int orderId, int companyId, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Shop)
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null) return null;

            return new OrderResponse
            {
                Id          = order.Id,
                ShopId      = order.ShopId,
                ShopName    = order.Shop.ShopName,
                CompanyId   = order.CompanyId,
                Status      = order.Status,
                TotalAmount = order.TotalAmount,
                Note        = order.Note,
                CreatedAt   = order.CreatedAt,
                UpdatedAt   = order.UpdatedAt,
                Items       = order.Items.Select(oi => new OrderItemResponse
                {
                    ProductId       = oi.ProductId,
                    ProductName     = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity        = oi.Quantity,
                    UnitPrice       = oi.UnitPrice,
                    Subtotal        = oi.Quantity * oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(
            int orderId, int companyId, OrderStatus newStatus, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            var oldStatus = order.Status;

            if (!AllowedTransitions.TryGetValue(oldStatus, out var allowed) ||
                !allowed.Contains(newStatus))
            {
                return (false,
                    $"'{oldStatus}' statusidan '{newStatus}' ga o'tish mumkin emas");
            }

            // Bekor qilinsa yoki rad etilsa — stock qaytariladi
            if (StockRestoreStatuses.Contains(newStatus))
            {
                var productIds = order.Items.Select(i => i.ProductId).ToList();
                var products   = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, ct);

                foreach (var item in order.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                        product.StockQuantity += item.Quantity;
                }
            }

            order.Status    = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Buyurtma #{OrderId} statusi {OldStatus} → {NewStatus}",
                orderId, oldStatus, newStatus);

            return (true, $"Buyurtma statusi '{newStatus}' ga o'zgartirildi");
        }
    }
}
