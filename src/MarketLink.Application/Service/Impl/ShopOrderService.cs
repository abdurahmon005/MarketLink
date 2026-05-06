using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using MarketLink.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly AppDbContext    _context;
        private readonly ICartRepository _cartRepo;
        private readonly ILogger<ShopOrderService> _logger;

        public ShopOrderService(
            AppDbContext context,
            ICartRepository cartRepo,
            ILogger<ShopOrderService> logger)
        {
            _context  = context;
            _cartRepo = cartRepo;
            _logger   = logger;
        }

        /// <summary>
        /// Savat → korxona bo'yicha guruhlash → har biri uchun Order yaratish.
        /// Stock tekshiruvi va kamaytirilishi tranzaksiyada amalga oshiriladi.
        /// </summary>
        public async Task<(bool Success, string Message, List<ShopOrderDto>? Orders)> CheckoutAsync(
            int shopId, CheckoutDto dto, CancellationToken ct = default)
        {
            // 1. Savatni olish
            var cartItems = await _cartRepo.GetByShopIdAsync(shopId, ct);

            if (!cartItems.Any())
                return (false, "Savat bo'sh", null);

            // 2. Barcha productlarni bir so'rovda yuklash (N+1 muammosini oldini olish)
            var productIds = cartItems.Select(c => c.ProductId).Distinct().ToList();
            var products   = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            // 3. Mavjudlik va qoldiq tekshiruvi
            foreach (var item in cartItems)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                    return (false,
                        $"'{item.Product?.Name ?? item.ProductId.ToString()}' mahsuloti topilmadi yoki faol emas",
                        null);

                if (item.Quantity > product.StockQuantity)
                    return (false,
                        $"'{product.Name}' uchun yetarli qoldiq yo'q (mavjud: {product.StockQuantity})",
                        null);
            }

            // 4. Mahsulotlarni CompanyId bo'yicha guruhlash
            var groups = cartItems.GroupBy(x => x.Product.CompanyId);

            var createdOrders = new List<Order>();

            // 5. Har bir korxona uchun alohida Order + stock kamaytirilishi
            foreach (var group in groups)
            {
                var order = new Order
                {
                    ShopId          = shopId,
                    CompanyId       = group.Key,
                    Status          = OrderStatus.Pending,
                    DeliveryDate    = dto.DeliveryDate.ToUniversalTime(),
                    DeliveryAddress = dto.DeliveryAddress,
                    Note            = dto.Note,
                    TotalAmount     = group.Sum(x => x.Product.Price * x.Quantity),
                    CreatedAt       = DateTime.UtcNow,
                    UpdatedAt       = DateTime.UtcNow,
                    Items           = group.Select(x => new OrderItem
                    {
                        ProductId = x.ProductId,
                        Quantity  = x.Quantity,
                        UnitPrice = x.Product.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                createdOrders.Add(order);

                // Stock kamaytirilishi
                foreach (var item in group)
                {
                    products[item.ProductId].StockQuantity -= item.Quantity;
                }
            }

            // 6. Saqlash
            await _context.SaveChangesAsync(ct);

            // 7. Savatni tozalash
            await _cartRepo.ClearAsync(shopId, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "ShopId {ShopId} checkout: {Count} ta order yaratildi",
                shopId, createdOrders.Count);

            // 8. Natija
            var orderIds  = createdOrders.Select(o => o.Id).ToList();
            var orderDtos = await BuildOrderDtosAsync(orderIds, ct);

            return (true, $"{createdOrders.Count} ta buyurtma yaratildi", orderDtos);
        }

        public async Task<PagedResult<ShopOrderGroupDto>> GetMyOrdersAsync(
            int shopId, ShopOrderFilterDto filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.Size is < 1 or > 50) filter.Size = 10;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.ShopId == shopId);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.CompanyId.HasValue)
                query = query.Where(o => o.CompanyId == filter.CompanyId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value.ToUniversalTime());

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.ToUniversalTime());

            var total = await query.CountAsync(ct);

            var orders = await query
                .Include(o => o.Company)
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .ToListAsync(ct);

            var orderIds = orders.Select(o => o.Id).ToList();
            var ratedSet = await _context.Ratings
                .AsNoTracking()
                .Where(r => r.ShopId == shopId && orderIds.Contains(r.OrderId))
                .Select(r => new { r.OrderId, r.ProductId })
                .ToListAsync(ct);

            var grouped = orders
                .GroupBy(o => new { Date = o.CreatedAt.Date, o.CompanyId })
                .Select(g => new ShopOrderGroupDto
                {
                    Date        = g.Key.Date,
                    CompanyId   = g.Key.CompanyId,
                    CompanyName = g.First().Company.CompanyName,
                    Orders      = g.Select(o => ToOrderDto(o, ratedSet
                        .Where(r => r.OrderId == o.Id)
                        .Select(r => r.ProductId)
                        .ToHashSet())).ToList()
                })
                .OrderByDescending(g => g.Date)
                .ToList();

            return new PagedResult<ShopOrderGroupDto>
            {
                Items      = grouped,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.Size
            };
        }

        public async Task<ShopOrderDto?> GetOrderDetailAsync(
            int shopId, int orderId, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Company)
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ShopId == shopId, ct);

            if (order == null) return null;

            var ratedProductsList = await _context.Ratings
                .AsNoTracking()
                .Where(r => r.ShopId == shopId && r.OrderId == orderId)
                .Select(r => r.ProductId)
                .ToListAsync(ct);

            return ToOrderDto(order, ratedProductsList.ToHashSet());
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(
            int shopId, int orderId, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ShopId == shopId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            if (order.Status != OrderStatus.Pending)
                return (false, "Faqat kutilayotgan (Pending) buyurtmani bekor qilish mumkin");

            // Stock qaytarish
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products   = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var item in order.Items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                    product.StockQuantity += item.Quantity;
            }

            order.Status    = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Buyurtma bekor qilindi");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<List<ShopOrderDto>> BuildOrderDtosAsync(
            List<int> orderIds, CancellationToken ct)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Company)
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync(ct);

            return orders.Select(o => ToOrderDto(o, new HashSet<int>())).ToList();
        }

        private static ShopOrderDto ToOrderDto(Order order, HashSet<int> ratedProductIds) => new()
        {
            OrderId         = order.Id,
            CompanyId       = order.CompanyId,
            CompanyName     = order.Company?.CompanyName ?? string.Empty,
            Status          = order.Status,
            TotalAmount     = order.TotalAmount,
            DeliveryDate    = order.DeliveryDate,
            DeliveryAddress = order.DeliveryAddress,
            Note            = order.Note,
            CreatedAt       = order.CreatedAt,
            Items           = order.Items.Select(oi => new ShopOrderItemDto
            {
                ProductId       = oi.ProductId,
                ProductName     = oi.Product?.Name ?? string.Empty,
                ProductImageUrl = oi.Product?.ImageUrl,
                Quantity        = oi.Quantity,
                UnitPrice       = oi.UnitPrice,
                SubTotal        = oi.Quantity * oi.UnitPrice,
                IsRated         = ratedProductIds.Contains(oi.ProductId)
            }).ToList()
        };
    }
}
