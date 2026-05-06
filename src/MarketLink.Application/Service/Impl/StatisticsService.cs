using MarketLink.Application.Models.Statistics;
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

            // Reytingi eng past mahsulot
            var lowestRated = await _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId && p.Ratings.Any())
                .Select(p => new
                {
                    p.Name,
                    Avg = p.Ratings.Average(r => (double)r.Score)
                })
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
    }
}
