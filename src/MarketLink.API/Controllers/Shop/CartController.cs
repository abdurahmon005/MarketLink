using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Cart;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Shop
{
    /// <summary>Savat boshqaruvi</summary>
    [ApiController]
    [Route("api/cart")]
    [Authorize(Roles = "Shop")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IValidator<AddToCartDto> _addValidator;

        public CartController(
            ICartService cartService,
            IValidator<AddToCartDto> addValidator)
        {
            _cartService  = cartService;
            _addValidator = addValidator;
        }

        /// <summary>Savatni ko'rish (korxona bo'yicha guruhlangan)</summary>
        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var cart = await _cartService.GetCartAsync(shopId.Value, ct);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Savat",
                Data    = cart
            });
        }

        /// <summary>Mahsulot qo'shish yoki miqdorni yangilash</summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToCartDto dto, CancellationToken ct)
        {
            var validation = await _addValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _cartService.AddOrUpdateItemAsync(shopId.Value, dto, ct);
            if (!success) return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Mahsulot miqdorini o'zgartirish</summary>
        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateItem(
            int productId,
            [FromBody] UpdateCartItemDto dto,
            CancellationToken ct)
        {
            if (dto.Quantity <= 0)
                return BadRequest(Fail("Miqdor 0 dan katta bo'lishi kerak"));

            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) =
                await _cartService.UpdateQuantityAsync(shopId.Value, productId, dto.Quantity, ct);
            if (!success) return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Mahsulotni savatdan o'chirish</summary>
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _cartService.RemoveItemAsync(shopId.Value, productId, ct);
            if (!success) return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Savatni to'liq tozalash</summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            await _cartService.ClearCartAsync(shopId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Savat tozalandi" });
        }

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }

        private static ApiResponse<object> Fail(string msg) =>
            new() { Success = false, Message = msg };

        private static ApiResponse<object> ValidationFail(
            FluentValidation.Results.ValidationResult v) => new()
        {
            Success = false,
            Message = "Validatsiya xatosi",
            Errors  = v.Errors.Select(e => e.ErrorMessage).ToList()
        };
    }
}
