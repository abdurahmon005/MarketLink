using MarketLink.Application.Models.Statistics;
using MarketLink.Application.Models.Supplier;

namespace MarketLink.Application.Service
{
    public interface IStatisticsService
    {
        /// <summary>Kunlik statistika</summary>
        Task<DailyStatisticsResponse> GetDailyAsync(
            int companyId, DateTime date, CancellationToken ct = default);

        /// <summary>Oylik statistika</summary>
        Task<MonthlyStatisticsResponse> GetMonthlyAsync(
            int companyId, int year, int month, CancellationToken ct = default);

        // ── Supplier Panel ────────────────────────────────────────────────────

        Task<SupplierDashboardStatsDto> GetSupplierStatsAsync(
            int companyId, string period, CancellationToken ct = default);

        Task<List<RevenueChartPointDto>> GetRevenueChartAsync(
            int companyId, string period, CancellationToken ct = default);

        Task<List<TopBuyerDto>> GetTopBuyersAsync(
            int companyId, string period, CancellationToken ct = default);

        Task<List<ActivityDto>> GetRecentActivityAsync(
            int companyId, int limit = 20, CancellationToken ct = default);
    }
}
