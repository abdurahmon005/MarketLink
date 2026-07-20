using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Rating;
using MarketLink.Application.Models.Supplier;

namespace MarketLink.Application.Service
{
    public interface IRatingService
    {
        Task<(bool Success, string Message)> RateProductAsync(
            int shopId, RateProductDto dto, CancellationToken ct = default);

        Task<PagedResult<RatingDto>> GetMyRatingsAsync(
            int shopId, int page, int size, CancellationToken ct = default);

        // ── Supplier Panel ────────────────────────────────────────────────────

        Task<RatingSummaryDto> GetProductRatingSummaryAsync(
            int productId, CancellationToken ct = default);

        Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(
            int productId, ReviewFilter filter, CancellationToken ct = default);

        Task<PagedResult<ProductReviewDto>> GetCompanyReviewsAsync(
            int companyId, ReviewFilter filter, CancellationToken ct = default);

        Task<(bool Success, string Message)> ReplyToReviewAsync(
            int reviewId, int companyId, string reply, CancellationToken ct = default);

        Task<RatingSummaryDto> GetCompanyRatingSummaryAsync(
            int companyId, CancellationToken ct = default);
    }
}
