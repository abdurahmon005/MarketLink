using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Shop
{
    /// <summary>Shop buyurtmalari (berish + kuzatish)</summary>
    [ApiController]
    [Route("api/orders")]
    [Authorize(Roles = "Shop")]
    public class ShopOrdersController : ControllerBase
    {
        private readonly IShopOrderService _orderService;
        private readonly IValidator<CheckoutDto> _checkoutValidator;

        public ShopOrdersController(
            IShopOrderService orderService,
            IValidator<CheckoutDto> checkoutValidator)
        {
            _orderService      = orderService;
            _checkoutValidator = checkoutValidator;
        }

        /// <summary>
        /// Savat → buyurtmaga aylantirish.
        /// Har bir korxona uchun alohida Order yaratiladi.
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(
            [FromBody] CheckoutDto dto, CancellationToken ct)
        {
            var validation = await _checkoutValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message, orders) = await _orderService.CheckoutAsync(shopId.Value, dto, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return StatusCode(201, new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data    = orders
            });
        }

        /// <summary>
        /// O'z buyurtmalar tarixi (filter + pagination).
        /// Natija kun + korxona bo'yicha guruhlanadi.
        /// </summary>
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

            var result = await _orderService.GetMyOrdersAsync(shopId.Value, filter, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Buyurtmalar tarixi",
                Data    = result
            });
        }

        /// <summary>Bitta buyurtma tafsiloti</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var order = await _orderService.GetOrderDetailAsync(shopId.Value, id, ct);
            if (order == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Buyurtma topilmadi yoki sizga tegishli emas"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Buyurtma tafsiloti",
                Data    = order
            });
        }

        /// <summary>Buyurtmani bekor qilish (faqat Pending holatda)</summary>
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _orderService.CancelOrderAsync(shopId.Value, id, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }

        private static ApiResponse<object> ValidationFail(
            FluentValidation.Results.ValidationResult v) => new()
        {
            Success = false,
            Message = "Validatsiya xatosi",
            Errors  = v.Errors.Select(e => e.ErrorMessage).ToList()
        };
    }
}
