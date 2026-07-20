using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Application.Models.Supplier;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
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

        // ── Supplier Panel Methods ─────────────────────────────────────────────

        public async Task<PagedResult<SupplierOrderListDto>> GetOrdersAsync(
            int companyId, SupplierOrderFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 50) filter.PageSize = 10;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new SupplierOrderListDto
                {
                    Id          = o.Id,
                    OrderNumber = $"#{o.Id:D6}",
                    ShopName    = o.Shop.ShopName,
                    ShopAddress = o.DeliveryAddress,
                    ItemCount   = o.Items.Count,
                    TotalPrice  = o.TotalAmount,
                    Status      = o.Status,
                    DeliveryDate = o.DeliveryDate,
                    CreatedAt   = o.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<SupplierOrderListDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<SupplierOrderDetailDto?> GetOrderDetailAsync(
            int orderId, int companyId, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Shop).ThenInclude(s => s.User)
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null) return null;

            var history = await _context.OrderStatusHistories
                .AsNoTracking()
                .Where(h => h.OrderId == orderId)
                .OrderBy(h => h.ChangedAt)
                .Select(h => new StatusHistoryDto
                {
                    Status    = h.Status,
                    ChangedAt = h.ChangedAt,
                    Note      = h.Note
                })
                .ToListAsync(ct);

            var tracking = await _context.DeliveryTrackings
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

            return new SupplierOrderDetailDto
            {
                Id           = order.Id,
                OrderNumber  = $"#{order.Id:D6}",
                ShopName     = order.Shop.ShopName,
                ShopAddress  = order.DeliveryAddress,
                ShopPhone    = order.Shop.User?.PhoneNumber,
                TotalPrice   = order.TotalAmount,
                Status       = order.Status,
                DeliveryDate = order.DeliveryDate,
                Notes        = order.Note,
                Items        = order.Items.Select(oi => new SupplierOrderItemDto
                {
                    ProductId       = oi.ProductId,
                    ProductName     = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity        = oi.Quantity,
                    UnitPrice       = oi.UnitPrice,
                    Subtotal        = oi.Quantity * oi.UnitPrice
                }).ToList(),
                StatusHistory = history,
                DriverInfo    = tracking == null ? null : new DriverInfoDto
                {
                    DriverId    = tracking.DriverId,
                    DriverName  = tracking.DriverName,
                    DriverPhone = tracking.DriverPhone,
                    Progress    = tracking.Progress,
                    EtaMinutes  = tracking.EtaMinutes
                }
            };
        }

        public async Task<(bool Success, string Message)> AcceptOrderAsync(
            int orderId, int companyId, Guid acceptedBy, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            if (order.Status != OrderStatus.Pending)
                return (false, "Faqat kutilayotgan buyurtmani qabul qilish mumkin");

            order.Status    = OrderStatus.Accepted;
            order.UpdatedAt = DateTime.UtcNow;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId   = orderId,
                Status    = OrderStatus.Accepted,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = acceptedBy,
                Note      = "Buyurtma qabul qilindi"
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Order #{OrderId} accepted by {UserId}", orderId, acceptedBy);
            return (true, "Buyurtma muvaffaqiyatli qabul qilindi");
        }

        public async Task<(bool Success, string Message)> RejectOrderAsync(
            int orderId, int companyId, string reason, Guid rejectedBy, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            if (order.Status != OrderStatus.Pending)
                return (false, "Faqat kutilayotgan buyurtmani rad etish mumkin");

            order.Status    = OrderStatus.Rejected;
            order.UpdatedAt = DateTime.UtcNow;

            // Restore stock
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products   = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var item in order.Items)
                if (products.TryGetValue(item.ProductId, out var product))
                    product.StockQuantity += item.Quantity;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId   = orderId,
                Status    = OrderStatus.Rejected,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = rejectedBy,
                Note      = reason
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Order #{OrderId} rejected by {UserId}: {Reason}", orderId, rejectedBy, reason);
            return (true, "Buyurtma rad etildi");
        }

        public async Task<(bool Success, string Message)> AssignDriverAsync(
            int orderId, int companyId, AssignDriverRequest request, Guid assignedBy, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Rejected
                || order.Status == OrderStatus.Cancelled)
                return (false, "Bu buyurtmaga haydovchi tayinlab bo'lmaydi");

            order.Status    = OrderStatus.Preparing;
            order.UpdatedAt = DateTime.UtcNow;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId   = orderId,
                Status    = OrderStatus.Preparing,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = assignedBy,
                Note      = $"Haydovchi tayinlandi: {request.DriverName}"
            });

            // Create or update DeliveryTracking
            var tracking = await _context.DeliveryTrackings
                .FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

            if (tracking == null)
            {
                tracking = new DeliveryTracking { OrderId = orderId };
                _context.DeliveryTrackings.Add(tracking);
            }

            tracking.DriverId       = request.DriverId;
            tracking.DriverName     = request.DriverName;
            tracking.DriverPhone    = request.DriverPhone;
            tracking.EtaMinutes     = request.EstimatedMinutes;
            tracking.Progress       = 10;
            tracking.LastUpdatedAt  = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Driver {DriverId} assigned to Order #{OrderId} by {UserId}",
                request.DriverId, orderId, assignedBy);

            return (true, "Haydovchi muvaffaqiyatli tayinlandi");
        }

        public async Task<int> GetNewOrdersCountAsync(int companyId, CancellationToken ct = default)
            => await _context.Orders
                .CountAsync(o => o.CompanyId == companyId && o.Status == OrderStatus.Pending, ct);
    }
}
