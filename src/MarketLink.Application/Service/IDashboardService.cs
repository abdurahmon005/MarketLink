using MarketLink.Application.Models.Dashboard;

namespace MarketLink.Application.Service
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync(int shopId, CancellationToken ct = default);
        Task<List<ActiveDeliveryCardDto>> GetActiveDeliveriesAsync(int shopId, CancellationToken ct = default);
    }
}
