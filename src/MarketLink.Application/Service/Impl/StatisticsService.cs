using MarketLink.Application.Models.Statistics;
using MarketLink.Application.Models.Supplier;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context) => _context = context;

        public async Task<DailyStatisticsResponse> GetDailyAsync(
            int companyId, DateTime date, CancellationToken ct = default)
        {
            var dayStart = date.Date.ToUniversalTime();
            var dayEnd   = dayStart.AddDays(1);

            var orderCount = await _context.Orders
                .CountAsync(o => o.CompanyId == companyId &&
                                 o.CreatedAt >= dayStart &&
                                 o.CreatedAt < dayEnd, ct);

            var soldProductCount = await _context.OrderItems
                .Where(oi => oi.Order.CompanyId == companyId &&
                             oi.Order.CreatedAt >= dayStart &&
                             oi.Order.CreatedAt < dayEnd &&
                             oi.Order.Status == OrderStatus.Delivered)
                .SumAsync(oi => (int?)oi.Quantity, ct) ?? 0;

            var totalRevenue = await _context.Orders
                .Where(o => o.CompanyId == companyId &&
                            o.CreatedAt >= dayStart &&
                            o.CreatedAt < dayEnd &&
                            o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            return new DailyStatisticsResponse
            {
                Date             = date.Date,
                OrderCount       = orderCount,
                SoldProductCount = soldProductCount,
                TotalRevenue     = totalRevenue
            };
        }

        public async Task<MonthlyStatisticsResponse> GetMonthlyAsync(
            int companyId, int year, int month, CancellationToken ct = default)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end   = start.AddMonths(1);

            var orderCount = await _context.Orders
                .CountAsync(o => o.CompanyId == companyId &&
                                 o.CreatedAt >= start &&
                                 o.CreatedAt < end, ct);

            var totalRevenue = await _context.Orders
                .Where(o => o.CompanyId == companyId &&
                            o.CreatedAt >= start &&
                            o.CreatedAt < end &&
                            o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            // Eng ko'p sotilgan mahsulot (DB-da aggregate)
            var topProduct = await _context.OrderItems
                .Where(oi => oi.Order.CompanyId == companyId &&
                             oi.Order.CreatedAt >= start &&
                             oi.Order.CreatedAt < end &&
                             oi.Order.Status == OrderStatus.Delivered)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new { g.Key.Name, Count = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

            // Reytingi eng past mahsulot (cached AverageRating ishlatiladi)
            var lowestRated = await _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId && p.AverageRating > 0)
                .Select(p => new { p.Name, Avg = p.AverageRating })
                .OrderBy(x => x.Avg)
                .FirstOrDefaultAsync(ct);

            return new MonthlyStatisticsResponse
            {
                Year                    = year,
                Month                   = month,
                OrderCount              = orderCount,
                TotalRevenue            = totalRevenue,
                TopProductName          = topProduct?.Name,
                TopProductSoldCount     = topProduct?.Count ?? 0,
                LowestRatedProductName  = lowestRated?.Name,
                LowestRatedProductScore = lowestRated?.Avg ?? 0
            };
        }

        // ── Supplier Panel Methods ─────────────────────────────────────────────

        public async Task<SupplierDashboardStatsDto> GetSupplierStatsAsync(
            int companyId, string period, CancellationToken ct = default)
        {
            var (from, to)     = GetPeriodRange(period);
            var (prevFrom, _)  = GetPeriodRange(period, previous: true);

            var currentRevenue = await _context.Orders
                .Where(o => o.CompanyId == companyId && o.Status == OrderStatus.Delivered
                         && o.CreatedAt >= from && o.CreatedAt < to)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            var prevRevenue = await _context.Orders
                .Where(o => o.CompanyId == companyId && o.Status == OrderStatus.Delivered
                         && o.CreatedAt >= prevFrom && o.CreatedAt < from)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            var currentOrders = await _context.Orders
                .CountAsync(o => o.CompanyId == companyId
                              && o.CreatedAt >= from && o.CreatedAt < to, ct);

            var prevOrders = await _context.Orders
                .CountAsync(o => o.CompanyId == companyId
                              && o.CreatedAt >= prevFrom && o.CreatedAt < from, ct);

            var avgRating = await _context.Ratings
                .Where(r => r.Product.CompanyId == companyId)
                .AverageAsync(r => (double?)r.Score, ct) ?? 0;

            var activeShops = await _context.Orders
                .Where(o => o.CompanyId == companyId
                         && o.CreatedAt >= from && o.CreatedAt < to)
                .Select(o => o.ShopId)
                .Distinct()
                .CountAsync(ct);

            var chart     = await GetRevenueChartAsync(companyId, period, ct);
            var topBuyers = await GetTopBuyersAsync(companyId, period, ct);
            var activity  = await GetRecentActivityAsync(companyId, 5, ct);

            var topProducts = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.CompanyId == companyId
                          && oi.Order.Status == OrderStatus.Delivered
                          && oi.Order.CreatedAt >= from && oi.Order.CreatedAt < to)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId  = g.Key.ProductId,
                    Name       = g.Key.Name,
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count(),
                    Revenue    = g.Sum(x => (decimal)x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync(ct);

            var totalRev = topProducts.Sum(p => p.Revenue);
            foreach (var p in topProducts)
                p.Percentage = totalRev > 0 ? Math.Round(p.Revenue / totalRev * 100, 1) : 0;

            return new SupplierDashboardStatsDto
            {
                Revenue        = currentRevenue,
                RevenueChange  = prevRevenue > 0
                    ? Math.Round((currentRevenue - prevRevenue) / prevRevenue * 100, 1) : 0,
                Orders         = currentOrders,
                OrdersChange   = prevOrders > 0
                    ? Math.Round((decimal)(currentOrders - prevOrders) / prevOrders * 100, 1) : 0,
                AvgRating      = Math.Round(avgRating, 2),
                ActiveShops    = activeShops,
                ChartData      = chart,
                TopProducts    = topProducts,
                TopBuyers      = topBuyers,
                RecentActivity = activity
            };
        }

        public async Task<List<RevenueChartPointDto>> GetRevenueChartAsync(
            int companyId, string period, CancellationToken ct = default)
        {
            var (from, to) = GetPeriodRange(period);

            // ToString("yyyy-MM-dd") EF Core da SQL ga translate bo'lmaydi — memory da qilinadi
            var raw = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId && o.Status == OrderStatus.Delivered
                         && o.CreatedAt >= from && o.CreatedAt < to)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount), OrderCount = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync(ct);

            return raw.Select(x => new RevenueChartPointDto
            {
                Date       = x.Date.ToString("yyyy-MM-dd"),
                Revenue    = x.Revenue,
                OrderCount = x.OrderCount
            }).ToList();
        }

        public async Task<List<TopBuyerDto>> GetTopBuyersAsync(
            int companyId, string period, CancellationToken ct = default)
        {
            var (from, to) = GetPeriodRange(period);

            var buyers = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId && o.Status == OrderStatus.Delivered
                         && o.CreatedAt >= from && o.CreatedAt < to)
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName, o.Shop.Address })
                .Select(g => new TopBuyerDto
                {
                    ShopId        = g.Key.ShopId,
                    ShopName      = g.Key.ShopName,
                    City          = g.Key.Address,
                    OrderCount    = g.Count(),
                    TotalSpent    = g.Sum(x => x.TotalAmount),
                    LastOrderDate = g.Max(x => x.CreatedAt)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync(ct);

            for (int i = 0; i < buyers.Count; i++)
                buyers[i].Rank = i + 1;

            return buyers;
        }

        public async Task<List<ActivityDto>> GetRecentActivityAsync(
            int companyId, int limit = 20, CancellationToken ct = default)
        {
            if (limit is < 1 or > 100) limit = 20;

            // CalcTimeAgo C# metodi SQL ga translate bo'lmaydi — memory da qilinadi
            var rows = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.UpdatedAt)
                .Take(limit)
                .Select(o => new { o.Id, o.Status, o.UpdatedAt })
                .ToListAsync(ct);

            return rows.Select(o => new ActivityDto
            {
                Id        = o.Id.ToString(),
                Type      = "order",
                Message   = $"#{o.Id:D6} buyurtma — {o.Status}",
                RelatedId = o.Id.ToString(),
                CreatedAt = o.UpdatedAt,
                TimeAgo   = CalcTimeAgo(o.UpdatedAt)
            }).ToList();
        }

        private static (DateTime From, DateTime To) GetPeriodRange(string period, bool previous = false)
        {
            var now = DateTime.UtcNow;
            (DateTime from, DateTime to) = period?.ToLowerInvariant() switch
            {
                "day"   => (now.Date, now.Date.AddDays(1)),
                "month" => (now.AddDays(-30), now),
                "year"  => (now.AddDays(-365), now),
                _       => (now.AddDays(-7), now)
            };

            if (!previous) return (from, to);

            var span = to - from;
            return (from - span, from);
        }

        private static string CalcTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1)  return "Hozirgina";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min oldin";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} soat oldin";
            if (diff.TotalDays    < 30) return $"{(int)diff.TotalDays} kun oldin";
            return $"{(int)(diff.TotalDays / 30)} oy oldin";
        }
    }
}
