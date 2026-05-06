using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Rating;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Shop
{
    /// <summary>Reyting berish va ko'rish</summary>
    [ApiController]
    [Route("api/ratings")]
    [Authorize(Roles = "Shop")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly IValidator<RateProductDto> _validator;

        public RatingsController(
            IRatingService ratingService,
            IValidator<RateProductDto> validator)
        {
            _ratingService = ratingService;
            _validator     = validator;
        }

        /// <summary>
        /// Mahsulotga reyting berish.
        /// Faqat Delivered buyurtmadagi mahsulotga, bir marta berish mumkin.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Rate(
            [FromBody] RateProductDto dto, CancellationToken ct)
        {
            var validation = await _validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var (success, message) = await _ratingService.RateProductAsync(shopId.Value, dto, ct);

            if (!success)
            {
                if (message.Contains("allaqachon"))
                    return Conflict(new ApiResponse<object> { Success = false, Message = message });

                return BadRequest(new ApiResponse<object> { Success = false, Message = message });
            }

            return StatusCode(201, new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>O'zim bergan reytinglar</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRatings(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var result = await _ratingService.GetMyRatingsAsync(shopId.Value, page, size, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Reytinglarim",
                Data    = result
            });
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
