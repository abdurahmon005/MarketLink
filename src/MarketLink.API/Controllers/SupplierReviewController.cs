using MarketLink.API.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/supplier")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierReviewController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public SupplierReviewController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetCompanyReviews(
            [FromQuery] ReviewFilter filter, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _ratingService.GetCompanyReviewsAsync(companyId.Value, filter, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Sharhlar ro'yxati", Data = result });
        }

        [HttpGet("reviews/summary")]
        public async Task<IActionResult> GetCompanySummary(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _ratingService.GetCompanyRatingSummaryAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Reyting xulosasi", Data = result });
        }

        [HttpPost("reviews/{id:int}/reply")]
        public async Task<IActionResult> ReplyToReview(
            int id, [FromBody] ReplyRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _ratingService.ReplyToReviewAsync(
                id, companyId.Value, request.Reply, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpGet("products/{productId:int}/reviews")]
        public async Task<IActionResult> GetProductReviews(
            int productId, [FromQuery] ReviewFilter filter, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _ratingService.GetProductReviewsAsync(productId, filter, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Mahsulot sharhlari", Data = result });
        }

        [HttpGet("products/{productId:int}/reviews/summary")]
        public async Task<IActionResult> GetProductSummary(int productId, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _ratingService.GetProductRatingSummaryAsync(productId, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Mahsulot reytingi xulosasi", Data = result });
        }

        private int? GetCompanyId()
        {
            var value = User.FindFirstValue("profile_id");
            return int.TryParse(value, out var id) ? id : null;
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
