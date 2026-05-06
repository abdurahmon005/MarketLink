using MarketLink.Application.Models.Statistics;

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
    }
}
