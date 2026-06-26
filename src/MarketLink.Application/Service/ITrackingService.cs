using MarketLink.Application.Models.Tracking;

namespace MarketLink.Application.Service
{
    public interface ITrackingService
    {
        Task<TrackingDto?> GetByOrderIdAsync(int orderId, CancellationToken ct = default);
        Task<List<ActiveDeliveryDto>> GetActiveAsync(int shopId, CancellationToken ct = default);
        Task<bool> UpdateLocationAsync(int orderId, UpdateLocationRequest req, CancellationToken ct = default);
        Task<bool> UpdateStatusAsync(int orderId, UpdateStatusRequest req, Guid changedBy, CancellationToken ct = default);
    }
}
