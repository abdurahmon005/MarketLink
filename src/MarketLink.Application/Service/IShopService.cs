using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Shop;
using MarketLink.Domain.Enums;

namespace MarketLink.Application.Service
{
    public interface IShopService
    {
        Task<PagedResult<ShopProfileResponse>> GetAllAsync(
            int page, int pageSize, ShopType? type = null, CancellationToken ct = default);

        Task<ShopProfileResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ShopProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateAsync(
            Guid userId, UpdateShopProfileRequest request, CancellationToken ct = default);
    }
}
