using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/company/orders")]
    [Authorize(Roles = "Company,Admin")]
    public class CompanyOrdersController : ControllerBase
    {
        private readonly ICompanyOrderService _orderService;

        public CompanyOrdersController(ICompanyOrderService orderService)
            => _orderService = orderService;

        /// <summary>Kelgan buyurtmalar ro'yxati</summary>
        [HttpGet("incoming")]
        public async Task<IActionResult> GetIncomingOrders(
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? shopName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var filter = new IncomingOrderFilter
            {
                Status   = status,
                DateFrom = dateFrom,
                DateTo   = dateTo,
                ShopName = shopName,
                Page     = page,
                PageSize = pageSize
            };

            var result = await _orderService.GetIncomingOrdersAsync(companyId.Value, filter, ct);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.Ok(result, "Kelgan buyurtmalar"));
        }

        /// <summary>Bitta buyurtma tafsiloti</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var order = await _orderService.GetOrderByIdAsync(id, companyId.Value, ct);
            if (order == null)
                return NotFound(ApiResponse<object>.Fail("Buyurtma topilmadi"));

            return Ok(ApiResponse<OrderResponse>.Ok(order, "Buyurtma tafsiloti"));
        }

        /// <summary>Buyurtma statusini o'zgartirish</summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) = await _orderService.UpdateStatusAsync(id, companyId.Value, request.Status, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
