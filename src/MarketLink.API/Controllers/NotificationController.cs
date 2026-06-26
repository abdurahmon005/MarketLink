using MarketLink.API.Common;
using MarketLink.Application.Models.Notification;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
            => _notificationService = notificationService;

        /// <summary>Get paginated notifications for the current user</summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool? isRead,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var filter = new NotificationFilter
            {
                IsRead   = isRead,
                Page     = page,
                PageSize = pageSize
            };

            var result = await _notificationService.GetAsync(userId.Value, filter, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Bildirishnomalar",
                Data    = result
            });
        }

        /// <summary>Mark a single notification as read</summary>
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkReadAsync(id, userId.Value, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = "O'qildi" });
        }

        /// <summary>Mark all notifications as read</summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllReadAsync(userId.Value, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = "Barchasi o'qildi" });
        }

        /// <summary>Register a push-notification device token</summary>
        [HttpPost("device-token")]
        public async Task<IActionResult> SaveDeviceToken(
            [FromBody] DeviceTokenRequest req, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.SaveDeviceTokenAsync(userId.Value, req.Token, req.Platform, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = "Qurilma token saqlandi" });
        }

        /// <summary>Remove a push-notification device token</summary>
        [HttpDelete("device-token/{token}")]
        public async Task<IActionResult> RemoveDeviceToken(string token, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.RemoveDeviceTokenAsync(userId.Value, token, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Qurilma token o'chirildi"
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Guid? GetCurrentUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }
}
