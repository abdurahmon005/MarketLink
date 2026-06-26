using MarketLink.Application.Models.Tracking;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class TrackingService : ITrackingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(AppDbContext context, ILogger<TrackingService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<TrackingDto?> GetByOrderIdAsync(int orderId, CancellationToken ct = default)
        {
            var tracking = await _context.DeliveryTrackings
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

            return tracking == null ? null : MapToDto(tracking);
        }

        public async Task<List<ActiveDeliveryDto>> GetActiveAsync(int shopId, CancellationToken ct = default)
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
                return new ActiveDeliveryDto
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

        public async Task<bool> UpdateLocationAsync(
            int orderId, UpdateLocationRequest req, CancellationToken ct = default)
        {
            var tracking = await _context.DeliveryTrackings
                .FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

            if (tracking == null)
            {
                tracking = new DeliveryTracking { OrderId = orderId };
                _context.DeliveryTrackings.Add(tracking);
            }

            tracking.CurrentLat      = req.Lat;
            tracking.CurrentLng      = req.Lng;
            tracking.CurrentLocation = req.CurrentLocation;
            tracking.DistanceLeft    = req.DistanceLeft;
            tracking.EtaMinutes      = req.EtaMinutes;
            tracking.LastUpdatedAt   = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(
            int orderId, UpdateStatusRequest req, Guid changedBy, CancellationToken ct = default)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
            if (order == null) return false;

            // Map DeliveryStatus to the closest OrderStatus
            order.Status = req.Status switch
            {
                DeliveryStatus.Confirming or
                DeliveryStatus.LeftWarehouse => OrderStatus.Accepted,
                DeliveryStatus.OnTheWay or
                DeliveryStatus.Nearby        => OrderStatus.Preparing,
                DeliveryStatus.Delivered     => OrderStatus.Delivered,
                DeliveryStatus.Cancelled     => OrderStatus.Cancelled,
                _                            => order.Status
            };
            order.UpdatedAt = DateTime.UtcNow;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId   = orderId,
                Status    = order.Status,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = changedBy,
                Note      = req.Note
            });

            // Update or create tracking record with new progress
            var tracking = await _context.DeliveryTrackings
                .FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

            if (tracking == null)
            {
                tracking = new DeliveryTracking { OrderId = orderId };
                _context.DeliveryTrackings.Add(tracking);
            }

            tracking.Progress = req.Status switch
            {
                DeliveryStatus.New           => 0,
                DeliveryStatus.Confirming    => 10,
                DeliveryStatus.LeftWarehouse => 30,
                DeliveryStatus.OnTheWay      => 60,
                DeliveryStatus.Nearby        => 85,
                DeliveryStatus.Delivered     => 100,
                DeliveryStatus.Cancelled     => 0,
                _                            => tracking.Progress
            };
            tracking.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Order {OrderId} status updated to {Status} by {UserId}",
                orderId, req.Status, changedBy);

            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static TrackingDto MapToDto(DeliveryTracking t) => new()
        {
            OrderId         = t.OrderId,
            Progress        = t.Progress,
            Status          = ProgressToStatus(t.Progress),
            CurrentLocation = t.CurrentLocation,
            CurrentLat      = t.CurrentLat,
            CurrentLng      = t.CurrentLng,
            DistanceLeft    = t.DistanceLeft,
            EtaMinutes      = t.EtaMinutes,
            DriverName      = t.DriverName,
            DriverPhone     = t.DriverPhone,
            LastUpdatedAt   = t.LastUpdatedAt
        };

        private static DeliveryStatus ProgressToStatus(int progress) => progress switch
        {
            0        => DeliveryStatus.New,
            <= 10    => DeliveryStatus.Confirming,
            <= 30    => DeliveryStatus.LeftWarehouse,
            <= 60    => DeliveryStatus.OnTheWay,
            < 100    => DeliveryStatus.Nearby,
            _        => DeliveryStatus.Delivered
        };
    }
}
