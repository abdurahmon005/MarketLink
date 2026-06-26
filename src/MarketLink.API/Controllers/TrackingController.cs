using MarketLink.API.Common;
using MarketLink.API.Hubs;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TrackingUpdateLocationRequest = MarketLink.Application.Models.Tracking.UpdateLocationRequest;
using TrackingUpdateStatusRequest   = MarketLink.Application.Models.Tracking.UpdateStatusRequest;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/tracking")]
    [Authorize]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService        _trackingService;
        private readonly INotificationService    _notificationService;
        private readonly IHubContext<TrackingHub> _hubContext;
        private readonly ILogger<TrackingController> _logger;

        public TrackingController(
            ITrackingService trackingService,
            INotificationService notificationService,
            IHubContext<TrackingHub> hubContext,
            ILogger<TrackingController> logger)
        {
            _trackingService     = trackingService;
            _notificationService = notificationService;
            _hubContext          = hubContext;
            _logger              = logger;
        }

        /// <summary>Active deliveries for the authenticated shop</summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var deliveries = await _trackingService.GetActiveAsync(shopId.Value, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Faol yetkazib berishlar",
                Data    = deliveries
            });
        }

        /// <summary>Tracking detail for a specific order</summary>
        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetTracking(int orderId, CancellationToken ct)
        {
            var tracking = await _trackingService.GetByOrderIdAsync(orderId, ct);

            if (tracking == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tracking ma'lumoti topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Tracking ma'lumotlari",
                Data    = tracking
            });
        }

        /// <summary>Update driver GPS location — emits SignalR TrackingUpdated</summary>
        [HttpPut("{orderId:int}/location")]
        public async Task<IActionResult> UpdateLocation(
            int orderId, [FromBody] TrackingUpdateLocationRequest req, CancellationToken ct)
        {
            await _trackingService.UpdateLocationAsync(orderId, req, ct);

            await _hubContext.Clients
                .Group($"order_{orderId}")
                .SendAsync("TrackingUpdated", new
                {
                    orderId,
                    currentLocation = req.CurrentLocation,
                    distanceLeft    = req.DistanceLeft,
                    etaMinutes      = req.EtaMinutes,
                    lat             = req.Lat,
                    lng             = req.Lng
                }, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = "Joylashuv yangilandi" });
        }

        /// <summary>Update delivery status — emits SignalR StatusChanged</summary>
        [HttpPut("{orderId:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int orderId, [FromBody] TrackingUpdateStatusRequest req, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await _trackingService.UpdateStatusAsync(orderId, req, userId.Value, ct);

            if (!success)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Buyurtma topilmadi"
                });

            var changedAt = DateTime.UtcNow;

            await _hubContext.Clients
                .Group($"order_{orderId}")
                .SendAsync("StatusChanged", new
                {
                    orderId,
                    newStatus = req.Status.ToString(),
                    changedAt,
                    message   = req.Note
                }, ct);

            if (req.Status == DeliveryStatus.Delivered)
            {
                await _hubContext.Clients
                    .Group($"order_{orderId}")
                    .SendAsync("OrderDelivered", new
                    {
                        orderId,
                        deliveredAt = changedAt
                    }, ct);
            }

            return Ok(new ApiResponse<object> { Success = true, Message = "Status yangilandi" });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }

        private Guid? GetCurrentUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }
}
