using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Rating;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    [Authorize(Roles = "Shop")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
            => _ratingService = ratingService;

        /// <summary>Mahsulotga reyting berish</summary>
        [HttpPost]
        public async Task<IActionResult> RateProduct(
            [FromBody] RateProductDto dto, CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _ratingService.RateProductAsync(shopId.Value, dto, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Do'konning o'z reytinglari</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRatings(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var result = await _ratingService.GetMyRatingsAsync(shopId.Value, page, size, ct);
            return Ok(ApiResponse<PagedResult<RatingDto>>.Ok(result, "Mening reytinglarim"));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
