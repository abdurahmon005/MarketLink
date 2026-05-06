using MarketLink.Application.Models.Cart;

namespace MarketLink.Application.Service
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int shopId, CancellationToken ct = default);

        Task<(bool Success, string Message)> AddOrUpdateItemAsync(
            int shopId, AddToCartDto dto, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateQuantityAsync(
            int shopId, int productId, int quantity, CancellationToken ct = default);

        Task<(bool Success, string Message)> RemoveItemAsync(
            int shopId, int productId, CancellationToken ct = default);

        Task ClearCartAsync(int shopId, CancellationToken ct = default);
    }
}
