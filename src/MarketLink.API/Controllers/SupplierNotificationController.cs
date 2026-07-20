using MarketLink.API.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/supplier/notifications")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierNotificationController : ControllerBase
    {
        private readonly ISupplierNotificationService _notificationService;

        public SupplierNotificationController(ISupplierNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] NotificationFilter filter, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _notificationService.GetAsync(companyId.Value, filter, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Bildirishnomalar", Data = result });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "O'qilmagan bildirishnomalar soni", Data = new { count } });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            await _notificationService.MarkReadAsync(id, companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Bildirishnoma o'qildi deb belgilandi" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            await _notificationService.MarkAllReadAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Barcha bildirishnomalar o'qildi deb belgilandi" });
        }

        private int? GetCompanyId()
        {
            var value = User.FindFirstValue("profile_id");
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
