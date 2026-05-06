using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Rating;

namespace MarketLink.Application.Service
{
    public interface IRatingService
    {
        Task<(bool Success, string Message)> RateProductAsync(
            int shopId, RateProductDto dto, CancellationToken ct = default);

        Task<PagedResult<RatingDto>> GetMyRatingsAsync(
            int shopId, int page, int size, CancellationToken ct = default);
    }
}
