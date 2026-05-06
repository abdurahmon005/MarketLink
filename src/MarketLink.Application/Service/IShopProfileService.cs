using MarketLink.Application.Models.Shop;
using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Service
{
    public interface IShopProfileService
    {
        Task<ShopProfileResponse?> GetProfileAsync(Guid userId, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateProfileAsync(
            Guid userId, UpdateShopProfileRequest request, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateMediaAsync(
            Guid userId, IFormFile? logo, IFormFile? certificate, CancellationToken ct = default);
    }
}
