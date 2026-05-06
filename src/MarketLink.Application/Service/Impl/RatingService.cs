using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Rating;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class RatingService : IRatingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RatingService> _logger;

        public RatingService(AppDbContext context, ILogger<RatingService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<(bool Success, string Message)> RateProductAsync(
            int shopId, RateProductDto dto, CancellationToken ct = default)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.ShopId == shopId, ct);

            if (order == null)
                return (false, "Buyurtma topilmadi yoki sizga tegishli emas");

            if (order.Status != OrderStatus.Delivered)
                return (false, "Faqat yetkazib berilgan buyurtmaga reyting berish mumkin");

            var orderItem = order.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (orderItem == null)
                return (false, "Bu mahsulot buyurtmada mavjud emas");

            var alreadyRated = await _context.Ratings
                .AnyAsync(r => r.ShopId == shopId
                            && r.ProductId == dto.ProductId
                            && r.OrderId == dto.OrderId, ct);

            if (alreadyRated)
                return (false, "Bu buyurtma uchun mahsulotga allaqachon reyting berilgan");

            if (dto.Score < 1 || dto.Score > 5)
                return (false, "Baho 1 dan 5 gacha bo'lishi kerak");

            var rating = new Rating
            {
                ShopId    = shopId,
                ProductId = dto.ProductId,
                OrderId   = dto.OrderId,
                Score     = dto.Score,
                Comment   = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync(ct);

            await RecalculateAverageRatingAsync(dto.ProductId, ct);

            _logger.LogInformation(
                "Reyting berildi: ShopId={ShopId}, ProductId={ProductId}, Score={Score}",
                shopId, dto.ProductId, dto.Score);

            return (true, "Reyting muvaffaqiyatli saqlandi");
        }

        public async Task<PagedResult<RatingDto>> GetMyRatingsAsync(
            int shopId, int page, int size, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (size is < 1 or > 50) size = 10;

            var query = _context.Ratings
                .AsNoTracking()
                .Where(r => r.ShopId == shopId);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(r => new RatingDto
                {
                    Id          = r.Id,
                    ProductId   = r.ProductId,
                    ProductName = r.Product.Name,
                    OrderId     = r.OrderId,
                    Score       = r.Score,
                    Comment     = r.Comment,
                    CreatedAt   = r.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<RatingDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = size
            };
        }

        private async Task RecalculateAverageRatingAsync(int productId, CancellationToken ct)
        {
            var product = await _context.Products.FindAsync(new object[] { productId }, ct);
            if (product == null) return;

            var avgScore = await _context.Ratings
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.Score, ct) ?? 0;

            product.AverageRating = avgScore;
            await _context.SaveChangesAsync(ct);
        }
    }
}
