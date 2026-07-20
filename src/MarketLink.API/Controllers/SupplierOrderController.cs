using MarketLink.API.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.Application.Service;
using MarketLink.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/supplier/orders")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierOrderController : ControllerBase
    {
        private readonly ICompanyOrderService _orderService;
        private readonly ISupplierNotificationService _supplierNotificationService;
        private readonly IHubContext<TrackingHub> _hub;

        public SupplierOrderController(
            ICompanyOrderService orderService,
            ISupplierNotificationService supplierNotificationService,
            IHubContext<TrackingHub> hub)
        {
            _orderService                = orderService;
            _supplierNotificationService = supplierNotificationService;
            _hub                         = hub;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] SupplierOrderFilter filter, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _orderService.GetOrdersAsync(companyId.Value, filter, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Buyurtmalar ro'yxati", Data = result });
        }

        [HttpGet("new-count")]
        public async Task<IActionResult> GetNewCount(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var count = await _orderService.GetNewOrdersCountAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Yangi buyurtmalar soni", Data = new { count } });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _orderService.GetOrderDetailAsync(id, companyId.Value, ct);
            if (result == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Buyurtma topilmadi" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Buyurtma ma'lumotlari", Data = result });
        }

        [HttpPut("{id:int}/accept")]
        public async Task<IActionResult> AcceptOrder(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _orderService.AcceptOrderAsync(id, companyId.Value, userId.Value, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            await _supplierNotificationService.SendAsync(
                companyId.Value,
                "Buyurtma qabul qilindi",
                $"#{id:D6} buyurtma muvaffaqiyatli qabul qilindi.",
                Domain.Enums.SupplierNotificationType.NewOrder,
                id, ct);

            await _hub.Clients.Group($"order_{id}")
                .SendAsync("StatusChanged", new { orderId = id, status = "Accepted" }, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> RejectOrder(
            int id, [FromBody] RejectOrderRequest request, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _orderService.RejectOrderAsync(
                id, companyId.Value, request.Reason, userId.Value, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            await _supplierNotificationService.SendAsync(
                companyId.Value,
                "Buyurtma rad etildi",
                $"#{id:D6} buyurtma rad etildi. Sabab: {request.Reason}",
                Domain.Enums.SupplierNotificationType.OrderCancelled,
                id, ct);

            await _hub.Clients.Group($"order_{id}")
                .SendAsync("StatusChanged", new { orderId = id, status = "Rejected" }, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPut("{id:int}/assign-driver")]
        public async Task<IActionResult> AssignDriver(
            int id, [FromBody] AssignDriverRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _orderService.AssignDriverAsync(
                id, companyId.Value, request, userId.Value, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            await _supplierNotificationService.SendAsync(
                companyId.Value,
                "Haydovchi tayinlandi",
                $"#{id:D6} buyurtmangiz yo'lda! Haydovchi: {request.DriverName}",
                Domain.Enums.SupplierNotificationType.NewOrder,
                id, ct);

            await _hub.Clients.Group($"order_{id}")
                .SendAsync("StatusChanged", new { orderId = id, status = "Preparing", driverName = request.DriverName }, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        private int? GetCompanyId()
        {
            var value = User.FindFirstValue("profile_id");
            return int.TryParse(value, out var id) ? id : null;
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private ApiResponse<object> ValidationError() => new()
        {
            Success = false,
            Message = "Ma'lumotlar noto'g'ri",
            Errors  = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()
        };
    }
}
