using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Rating;
using MarketLink.Application.Models.Supplier;
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

        // ── Supplier Panel Methods ─────────────────────────────────────────────

        public async Task<RatingSummaryDto> GetProductRatingSummaryAsync(
            int productId, CancellationToken ct = default)
        {
            var ratings = await _context.Ratings
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Select(r => r.Score)
                .ToListAsync(ct);

            return BuildSummary(ratings);
        }

        public async Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(
            int productId, ReviewFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 50) filter.PageSize = 10;

            var query = _context.Ratings
                .AsNoTracking()
                .Where(r => r.ProductId == productId);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Score == filter.Rating.Value);

            if (filter.Answered.HasValue)
                query = filter.Answered.Value
                    ? query.Where(r => r.SupplierReply != null)
                    : query.Where(r => r.SupplierReply == null);

            var total = await query.CountAsync(ct);

            var rows = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new
                {
                    r.Id, r.Score, r.Comment, r.SupplierReply, r.RepliedAt, r.CreatedAt,
                    ShopName    = r.Shop.ShopName,
                    ProductName = r.Product.Name
                })
                .ToListAsync(ct);

            var items = rows.Select(r => new ProductReviewDto
            {
                Id            = r.Id,
                ShopName      = r.ShopName,
                ProductName   = r.ProductName,
                Rating        = r.Score,
                Comment       = r.Comment,
                SupplierReply = r.SupplierReply,
                RepliedAt     = r.RepliedAt,
                CreatedAt     = r.CreatedAt,
                TimeAgo       = CalcTimeAgo(r.CreatedAt)
            }).ToList();

            return new PagedResult<ProductReviewDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<PagedResult<ProductReviewDto>> GetCompanyReviewsAsync(
            int companyId, ReviewFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 50) filter.PageSize = 10;

            var query = _context.Ratings
                .AsNoTracking()
                .Where(r => r.Product.CompanyId == companyId);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Score == filter.Rating.Value);

            if (filter.Answered.HasValue)
                query = filter.Answered.Value
                    ? query.Where(r => r.SupplierReply != null)
                    : query.Where(r => r.SupplierReply == null);

            var total = await query.CountAsync(ct);

            var rows2 = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new
                {
                    r.Id, r.Score, r.Comment, r.SupplierReply, r.RepliedAt, r.CreatedAt,
                    ShopName    = r.Shop.ShopName,
                    ProductName = r.Product.Name
                })
                .ToListAsync(ct);

            var items2 = rows2.Select(r => new ProductReviewDto
            {
                Id            = r.Id,
                ShopName      = r.ShopName,
                ProductName   = r.ProductName,
                Rating        = r.Score,
                Comment       = r.Comment,
                SupplierReply = r.SupplierReply,
                RepliedAt     = r.RepliedAt,
                CreatedAt     = r.CreatedAt,
                TimeAgo       = CalcTimeAgo(r.CreatedAt)
            }).ToList();

            return new PagedResult<ProductReviewDto>
            {
                Items      = items2,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<(bool Success, string Message)> ReplyToReviewAsync(
            int reviewId, int companyId, string reply, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reply))
                return (false, "Javob bo'sh bo'lmasligi kerak");

            if (reply.Length > 500)
                return (false, "Javob 500 belgidan oshmasligi kerak");

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.Product.CompanyId == companyId, ct);

            if (rating == null)
                return (false, "Reyting topilmadi yoki sizga tegishli emas");

            if (rating.SupplierReply != null)
                return (false, "Bu reytingga allaqachon javob berilgan");

            rating.SupplierReply = reply;
            rating.RepliedAt     = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Javob muvaffaqiyatli saqlandi");
        }

        public async Task<RatingSummaryDto> GetCompanyRatingSummaryAsync(
            int companyId, CancellationToken ct = default)
        {
            var ratings = await _context.Ratings
                .AsNoTracking()
                .Where(r => r.Product.CompanyId == companyId)
                .Select(r => r.Score)
                .ToListAsync(ct);

            return BuildSummary(ratings);
        }

        private static RatingSummaryDto BuildSummary(List<int> scores)
        {
            if (!scores.Any())
                return new RatingSummaryDto { AverageRating = 0, TotalCount = 0 };

            return new RatingSummaryDto
            {
                AverageRating = Math.Round(scores.Average(), 2),
                TotalCount    = scores.Count,
                Breakdown     = new RatingCountDto
                {
                    Five  = scores.Count(s => s == 5),
                    Four  = scores.Count(s => s == 4),
                    Three = scores.Count(s => s == 3),
                    Two   = scores.Count(s => s == 2),
                    One   = scores.Count(s => s == 1)
                }
            };
        }

        private static string CalcTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;
            if (diff.TotalMinutes < 1)  return "Hozirgina";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min oldin";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} soat oldin";
            if (diff.TotalDays    < 30) return $"{(int)diff.TotalDays} kun oldin";
            return $"{(int)(diff.TotalDays / 30)} oy oldin";
        }
    }
}
