using MarketLink.Domain.Entities;

namespace MarketLink.Domain.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetByShopIdAsync(int shopId, CancellationToken ct = default);
        Task<CartItem?> GetItemAsync(int shopId, int productId, CancellationToken ct = default);
        Task AddOrUpdateAsync(CartItem item, CancellationToken ct = default);
        Task RemoveAsync(int shopId, int productId, CancellationToken ct = default);
        Task ClearAsync(int shopId, CancellationToken ct = default);
    }
}
