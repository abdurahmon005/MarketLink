using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.DataAccess.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) => _context = context;

        public async Task<List<CartItem>> GetByShopIdAsync(int shopId, CancellationToken ct = default)
            => await _context.CartItems
                .Include(c => c.Product).ThenInclude(p => p.Company)
                .Where(c => c.ShopId == shopId)
                .OrderBy(c => c.AddedAt)
                .ToListAsync(ct);

        public async Task<CartItem?> GetItemAsync(int shopId, int productId, CancellationToken ct = default)
            => await _context.CartItems
                .FirstOrDefaultAsync(c => c.ShopId == shopId && c.ProductId == productId, ct);

        public async Task AddOrUpdateAsync(CartItem item, CancellationToken ct = default)
        {
            var existing = await GetItemAsync(item.ShopId, item.ProductId, ct);
            if (existing == null)
            {
                _context.CartItems.Add(item);
            }
            else
            {
                existing.Quantity  = item.Quantity;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task RemoveAsync(int shopId, int productId, CancellationToken ct = default)
        {
            var item = await GetItemAsync(shopId, productId, ct);
            if (item != null) _context.CartItems.Remove(item);
        }

        public async Task ClearAsync(int shopId, CancellationToken ct = default)
        {
            var items = await _context.CartItems
                .Where(c => c.ShopId == shopId)
                .ToListAsync(ct);
            _context.CartItems.RemoveRange(items);
        }
    }
}
