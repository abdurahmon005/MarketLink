using MarketLink.Application.Models.Catalog;
using MarketLink.Application.Models.Common;
using MarketLink.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class CatalogService : ICatalogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(AppDbContext context, ILogger<CatalogService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<PagedResult<CatalogProductDto>> GetProductsAsync(
            CatalogFilterDto filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.Size is < 1 or > 100) filter.Size = 20;

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Company)
                .Include(p => p.Ratings)
                .AsQueryable();

            if (filter.CompanyId.HasValue)
                query = query.Where(p => p.CompanyId == filter.CompanyId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.Direction.HasValue)
                query = query.Where(p => p.Company.ProductionType == filter.Direction.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{filter.Search}%"));

            // Saralash
            query = (filter.SortBy?.ToLower(), filter.SortOrder?.ToLower()) switch
            {
                ("price",     "asc")  => query.OrderBy(p => p.Price),
                ("price",     _)      => query.OrderByDescending(p => p.Price),
                ("name",      "asc")  => query.OrderBy(p => p.Name),
                ("name",      _)      => query.OrderByDescending(p => p.Name),
                ("rating",    "asc")  => query.OrderBy(p => p.Ratings.Average(r => (double?)r.Score) ?? 0),
                ("rating",    _)      => query.OrderByDescending(p => p.Ratings.Average(r => (double?)r.Score) ?? 0),
                ("createdat", "asc")  => query.OrderBy(p => p.CreatedAt),
                _                     => query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .Select(p => new CatalogProductDto
                {
                    Id               = p.Id,
                    Name             = p.Name,
                    CompanyId        = p.CompanyId,
                    CompanyName      = p.Company.CompanyName,
                    CompanyDirection = p.Company.ProductionType,
                    Price            = p.Price,
                    ImageUrl         = p.ImageUrl,
                    PackageSize      = p.PackageSize,
                    StockQuantity    = p.StockQuantity,
                    AverageRating    = p.Ratings.Any()
                        ? p.Ratings.Average(r => (double)r.Score) : 0,
                    RatingCount      = p.Ratings.Count,
                    CreatedAt        = p.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<CatalogProductDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.Size
            };
        }

        public async Task<CatalogProductDetailDto?> GetProductByIdAsync(
            int productId, CancellationToken ct = default)
        {
            var p = await _context.Products
                .AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.Ratings).ThenInclude(r => r.Shop)
                .FirstOrDefaultAsync(x => x.Id == productId && x.IsActive, ct);

            if (p == null) return null;

            return new CatalogProductDetailDto
            {
                Id               = p.Id,
                Name             = p.Name,
                Description      = p.Description,
                CompanyId        = p.CompanyId,
                CompanyName      = p.Company.CompanyName,
                CompanyDirection = p.Company.ProductionType,
                Price            = p.Price,
                ImageUrl         = p.ImageUrl,
                PackageSize      = p.PackageSize,
                StockQuantity    = p.StockQuantity,
                AverageRating    = p.Ratings.Any()
                    ? p.Ratings.Average(r => (double)r.Score) : 0,
                RatingCount      = p.Ratings.Count,
                CreatedAt        = p.CreatedAt,
                Ratings          = p.Ratings.Select(r => new CatalogRatingDto
                {
                    ShopId    = r.ShopId,
                    ShopName  = r.Shop.ShopName,
                    Score     = r.Score,
                    Comment   = r.Comment,
                    CreatedAt = r.CreatedAt
                }).OrderByDescending(r => r.CreatedAt).ToList()
            };
        }
    }
}
