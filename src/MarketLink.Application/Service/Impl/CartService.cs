using MarketLink.Application.Models.Cart;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class CartService : ICartService
    {
        private readonly AppDbContext  _context;
        private readonly ICartRepository _cartRepo;
        private readonly ILogger<CartService> _logger;

        public CartService(
            AppDbContext context,
            ICartRepository cartRepo,
            ILogger<CartService> logger)
        {
            _context  = context;
            _cartRepo = cartRepo;
            _logger   = logger;
        }

        public async Task<CartDto> GetCartAsync(int shopId, CancellationToken ct = default)
        {
            var items = await _cartRepo.GetByShopIdAsync(shopId, ct);

            var itemDtos = items.Select(c => new CartItemDto
            {
                ProductId   = c.ProductId,
                ProductName = c.Product.Name,
                CompanyId   = c.Product.CompanyId,
                CompanyName = c.Product.Company.CompanyName,
                Price       = c.Product.Price,
                Quantity    = c.Quantity,
                SubTotal    = c.Product.Price * c.Quantity,
                ImageUrl    = c.Product.ImageUrl,
                StockQuantity = c.Product.StockQuantity
            }).ToList();

            var grouped = itemDtos
                .GroupBy(i => new { i.CompanyId, i.CompanyName })
                .Select(g => new CompanyCartGroup
                {
                    CompanyId   = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    Items       = g.ToList(),
                    GroupTotal  = g.Sum(x => x.SubTotal)
                }).ToList();

            return new CartDto
            {
                Items            = itemDtos,
                TotalPrice       = itemDtos.Sum(i => i.SubTotal),
                GroupedByCompany = grouped
            };
        }

        public async Task<(bool Success, string Message)> AddOrUpdateItemAsync(
            int shopId, AddToCartDto dto, CancellationToken ct = default)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki faol emas");

            if (dto.Quantity <= 0)
                return (false, "Miqdor 0 dan katta bo'lishi kerak");

            if (dto.Quantity > product.StockQuantity)
                return (false, $"Mavjud qoldiq: {product.StockQuantity}");

            var item = new CartItem
            {
                ShopId    = shopId,
                ProductId = dto.ProductId,
                Quantity  = dto.Quantity,
                AddedAt   = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cartRepo.AddOrUpdateAsync(item, ct);
            await _context.SaveChangesAsync(ct);

            return (true, "Savat yangilandi");
        }

        public async Task<(bool Success, string Message)> UpdateQuantityAsync(
            int shopId, int productId, int quantity, CancellationToken ct = default)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi");

            if (quantity <= 0)
                return (false, "Miqdor 0 dan katta bo'lishi kerak");

            if (quantity > product.StockQuantity)
                return (false, $"Mavjud qoldiq: {product.StockQuantity}");

            var item = new CartItem
            {
                ShopId    = shopId,
                ProductId = productId,
                Quantity  = quantity,
                UpdatedAt = DateTime.UtcNow
            };

            await _cartRepo.AddOrUpdateAsync(item, ct);
            await _context.SaveChangesAsync(ct);

            return (true, "Miqdor yangilandi");
        }

        public async Task<(bool Success, string Message)> RemoveItemAsync(
            int shopId, int productId, CancellationToken ct = default)
        {
            await _cartRepo.RemoveAsync(shopId, productId, ct);
            await _context.SaveChangesAsync(ct);
            return (true, "Mahsulot savatdan o'chirildi");
        }

        public async Task ClearCartAsync(int shopId, CancellationToken ct = default)
        {
            await _cartRepo.ClearAsync(shopId, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
