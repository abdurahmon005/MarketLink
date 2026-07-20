using MarketLink.Application.Models.Cart;
using MarketLink.Application.Models.Common;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(Roles = "Shop")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
            => _cartService = cartService;

        /// <summary>Savatni ko'rish</summary>
        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var cart = await _cartService.GetCartAsync(shopId.Value, ct);
            return Ok(ApiResponse<CartDto>.Ok(cart, "Savat"));
        }

        /// <summary>Savatga mahsulot qo'shish yoki yangilash</summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddOrUpdateItem(
            [FromBody] AddToCartDto dto, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _cartService.AddOrUpdateItemAsync(shopId.Value, dto, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Savatdagi mahsulot miqdorini yangilash</summary>
        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateQuantity(
            int productId, [FromBody] UpdateCartItemDto dto, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _cartService.UpdateQuantityAsync(shopId.Value, productId, dto.Quantity, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Savatdan mahsulot o'chirish</summary>
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _cartService.RemoveItemAsync(shopId.Value, productId, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Savatni tozalash</summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            await _cartService.ClearCartAsync(shopId.Value, ct);
            return Ok(ApiResponse<object>.Ok(null!, "Savat tozalandi"));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
