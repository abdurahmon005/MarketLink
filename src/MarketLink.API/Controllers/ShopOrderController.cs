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
    [Route("api/orders")]
    [Authorize(Roles = "Shop")]
    public class ShopOrderController : ControllerBase
    {
        private readonly IShopOrderService _shopOrderService;

        public ShopOrderController(IShopOrderService shopOrderService)
            => _shopOrderService = shopOrderService;

        /// <summary>Checkout — savatdan buyurtma yaratish</summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(
            [FromBody] CheckoutDto dto, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message, orders) = await _shopOrderService.CheckoutAsync(shopId.Value, dto, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<List<ShopOrderDto>>.Ok(orders!, message));
        }

        /// <summary>Do'konning buyurtmalar tarixi</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] OrderStatus? status,
            [FromQuery] int? companyId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var filter = new ShopOrderFilterDto
            {
                Status    = status,
                CompanyId = companyId,
                FromDate  = fromDate,
                ToDate    = toDate,
                Page      = page,
                Size      = size
            };

            var result = await _shopOrderService.GetMyOrdersAsync(shopId.Value, filter, ct);
            return Ok(ApiResponse<PagedResult<ShopOrderGroupDto>>.Ok(result, "Buyurtmalar"));
        }

        /// <summary>Bitta buyurtma tafsiloti</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderDetail(int id, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var order = await _shopOrderService.GetOrderDetailAsync(shopId.Value, id, ct);
            if (order == null)
                return NotFound(ApiResponse<object>.Fail("Buyurtma topilmadi"));

            return Ok(ApiResponse<ShopOrderDto>.Ok(order, "Buyurtma tafsiloti"));
        }

        /// <summary>Buyurtmani bekor qilish</summary>
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _shopOrderService.CancelOrderAsync(shopId.Value, id, ct);
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
