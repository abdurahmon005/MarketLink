using MarketLink.Application.Models.Dashboard;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) => _context = context;

        public async Task<DashboardStatsDto> GetStatsAsync(int shopId, CancellationToken ct = default)
        {
            var now        = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd   = monthStart.AddMonths(1);

            var monthOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.ShopId == shopId &&
                            o.CreatedAt >= monthStart &&
                            o.CreatedAt < monthEnd)
                .ToListAsync(ct);

            var thisMonth = new MonthStatsDto
            {
                TotalSpent     = monthOrders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .Sum(o => o.TotalAmount),
                OrderCount     = monthOrders.Count,
                DeliveredCount = monthOrders.Count(o => o.Status == OrderStatus.Delivered),
                PendingCount   = monthOrders.Count(o =>
                    o.Status == OrderStatus.Pending || o.Status == OrderStatus.Accepted)
            };

            // Daily chart grouped by date
            var chart = monthOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ChartPointDto
                {
                    Date   = g.Key,
                    Spent  = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                    Orders = g.Count()
                })
                .ToList();

            // Top 5 products ordered this month
            var topProducts = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.ShopId == shopId &&
                             oi.Order.CreatedAt >= monthStart &&
                             oi.Order.CreatedAt < monthEnd)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId  = g.Key.ProductId,
                    Name       = g.Key.Name,
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count(),
                    TotalQty   = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(5)
                .ToListAsync(ct);

            return new DashboardStatsDto
            {
                ThisMonth   = thisMonth,
                Chart       = chart,
                TopProducts = topProducts
            };
        }

        public async Task<List<ActiveDeliveryCardDto>> GetActiveDeliveriesAsync(
            int shopId, CancellationToken ct = default)
        {
            var activeStatuses = new[] { OrderStatus.Accepted, OrderStatus.Preparing };

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.ShopId == shopId && activeStatuses.Contains(o.Status))
                .Include(o => o.Company)
                .Include(o => o.Items)
                .ToListAsync(ct);

            var orderIds = orders.Select(o => o.Id).ToList();
            var trackings = await _context.DeliveryTrackings
                .AsNoTracking()
                .Where(t => orderIds.Contains(t.OrderId))
                .ToDictionaryAsync(t => t.OrderId, ct);

            return orders.Select(o =>
            {
                trackings.TryGetValue(o.Id, out var tracking);
                return new ActiveDeliveryCardDto
                {
                    OrderId      = o.Id,
                    OrderNumber  = $"#{o.Id:D6}",
                    SupplierName = o.Company?.CompanyName ?? string.Empty,
                    Progress     = tracking?.Progress ?? 0,
                    EtaMinutes   = tracking?.EtaMinutes ?? 0,
                    ItemCount    = o.Items.Count
                };
            }).ToList();
        }
    }
}
