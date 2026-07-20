using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/shop/profile")]
    [Authorize(Roles = "Shop")]
    public class ShopProfileController : ControllerBase
    {
        private readonly IShopProfileService _shopProfileService;

        public ShopProfileController(IShopProfileService shopProfileService)
            => _shopProfileService = shopProfileService;

        /// <summary>Do'kon profilini olish</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Forbid();

            var profile = await _shopProfileService.GetProfileAsync(userId.Value, ct);
            if (profile == null)
                return NotFound(ApiResponse<object>.Fail("Profil topilmadi"));

            return Ok(ApiResponse<ShopProfileResponse>.Ok(profile, "Do'kon profili"));
        }

        /// <summary>Do'kon profilini yangilash</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateShopProfileRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Forbid();

            var (success, message) = await _shopProfileService.UpdateProfileAsync(userId.Value, request, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Do'kon logo va sertifikatini yangilash</summary>
        [HttpPut("media")]
        public async Task<IActionResult> UpdateMedia(
            IFormFile? logo, IFormFile? certificate, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Forbid();

            var (success, message) = await _shopProfileService.UpdateMediaAsync(userId.Value, logo, certificate, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Guid? GetUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }
}
