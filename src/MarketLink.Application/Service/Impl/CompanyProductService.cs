using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Product;
using MarketLink.Application.Models.Rating;
using MarketLink.Application.Models.Supplier;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class CompanyProductService : ICompanyProductService
    {
        private readonly AppDbContext  _context;
        private readonly IFileService  _fileService;
        private readonly ILogger<CompanyProductService> _logger;

        public CompanyProductService(
            AppDbContext context,
            IFileService fileService,
            ILogger<CompanyProductService> logger)
        {
            _context     = context;
            _fileService = fileService;
            _logger      = logger;
        }

        public async Task<(bool Success, string Message, ProductResponse? Data)> CreateAsync(
            int companyId, CreateProductRequest request, CancellationToken ct = default)
        {
            string? imageUrl = null;

            if (request.Image != null)
            {
                try
                {
                    var upload = await _fileService.UploadAsync(
                        request.Image, FileType.ProductImage,
                        Guid.NewGuid(), ct);
                    imageUrl = upload.Url ?? upload.ObjectPath;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Rasm yuklashda xato");
                }
            }

            var product = new Domain.Entities.Product
            {
                CompanyId     = companyId,
                Name          = request.Name,
                Description   = request.Description,
                ImageUrl      = imageUrl,
                Price         = request.Price,
                PackageSize   = request.PackageSize,
                StockQuantity = request.StockQuantity,
                IsActive      = true,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Yangi mahsulot yaratildi: {ProductId}, CompanyId: {CompanyId}",
                product.Id, companyId);

            return (true, "Mahsulot muvaffaqiyatli qo'shildi", ToResponse(product));
        }

        public async Task<PagedResult<ProductResponse>> GetMyProductsAsync(
            int companyId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponse
                {
                    Id            = p.Id,
                    CompanyId     = p.CompanyId,
                    Name          = p.Name,
                    Description   = p.Description,
                    ImageUrl      = p.ImageUrl,
                    Price         = p.Price,
                    PackageSize   = p.PackageSize,
                    StockQuantity = p.StockQuantity,
                    IsActive      = p.IsActive,
                    AverageRating = p.Ratings.Any()
                        ? p.Ratings.Average(r => (double)r.Score) : 0,
                    RatingCount   = p.Ratings.Count,
                    CreatedAt     = p.CreatedAt,
                    UpdatedAt     = p.UpdatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<ProductResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        public async Task<ProductResponse?> GetByIdAsync(
            int productId, int companyId, CancellationToken ct = default)
        {
            var p = await _context.Products
                .AsNoTracking()
                .Include(x => x.Ratings)
                .FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, ct);

            return p == null ? null : ToResponse(p);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            int productId, int companyId, UpdateProductRequest request, CancellationToken ct = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki sizga tegishli emas");

            if (request.Name          != null) product.Name          = request.Name;
            if (request.Description   != null) product.Description   = request.Description;
            if (request.Price         != null) product.Price         = request.Price.Value;
            if (request.PackageSize   != null) product.PackageSize   = request.PackageSize.Value;
            if (request.StockQuantity != null) product.StockQuantity = request.StockQuantity.Value;
            if (request.IsActive      != null) product.IsActive      = request.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Mahsulot yangilandi");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(
            int productId, int companyId, CancellationToken ct = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki sizga tegishli emas");

            var hasActiveOrders = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId &&
                    (oi.Order.Status == OrderStatus.Pending ||
                     oi.Order.Status == OrderStatus.Accepted ||
                     oi.Order.Status == OrderStatus.Preparing), ct);

            if (hasActiveOrders)
                return (false, "Mahsulotni o'chirib bo'lmaydi: faol buyurtmalarda mavjud");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(ct);

            return (true, "Mahsulot o'chirildi");
        }

        public async Task<(bool Success, string Message, string? ImageUrl)> UpdateImageAsync(
            int productId, int companyId, IFormFile image, CancellationToken ct = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki sizga tegishli emas", null);

            try
            {
                var upload = await _fileService.UploadAsync(
                    image, FileType.ProductImage, Guid.NewGuid(), ct);

                product.ImageUrl  = upload.Url ?? upload.ObjectPath;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);

                return (true, "Rasm yangilandi", product.ImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rasm yangilashda xato. ProductId: {ProductId}", productId);
                return (false, "Rasmni yuklashda xato yuz berdi", null);
            }
        }

        public async Task<List<ProductStockResponse>> GetStockAsync(
            int companyId, CancellationToken ct = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId)
                .Select(p => new ProductStockResponse
                {
                    ProductId     = p.Id,
                    Name          = p.Name,
                    StockQuantity = p.StockQuantity,
                    SoldQuantity  = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                        .Sum(oi => oi.Quantity),
                    AverageRating = p.Ratings.Any()
                        ? p.Ratings.Average(r => (double)r.Score) : 0,
                    IsActive      = p.IsActive
                })
                .OrderBy(p => p.StockQuantity)
                .ToListAsync(ct);
        }

        public async Task<PagedResult<ProductRatingResponse>> GetRatingsAsync(
            int companyId, int? productId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Ratings
                .AsNoTracking()
                .Where(r => r.Product.CompanyId == companyId);

            if (productId.HasValue)
                query = query.Where(r => r.ProductId == productId.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ProductRatingResponse
                {
                    Id          = r.Id,
                    ProductId   = r.ProductId,
                    ProductName = r.Product.Name,
                    ShopId      = r.ShopId,
                    ShopName    = r.Shop.ShopName,
                    Score       = r.Score,
                    Comment     = r.Comment,
                    CreatedAt   = r.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<ProductRatingResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        public async Task<ProductAverageRatingResponse?> GetAverageRatingAsync(
            int productId, int companyId, CancellationToken ct = default)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null) return null;

            return new ProductAverageRatingResponse
            {
                ProductId    = product.Id,
                ProductName  = product.Name,
                AverageScore = product.Ratings.Any()
                    ? product.Ratings.Average(r => (double)r.Score) : 0,
                TotalRatings = product.Ratings.Count
            };
        }

        private static ProductResponse ToResponse(Domain.Entities.Product p) => new()
        {
            Id            = p.Id,
            CompanyId     = p.CompanyId,
            Name          = p.Name,
            Description   = p.Description,
            ImageUrl      = p.ImageUrl,
            Price         = p.Price,
            PackageSize   = p.PackageSize,
            StockQuantity = p.StockQuantity,
            IsActive      = p.IsActive,
            AverageRating = p.Ratings?.Any() == true
                ? p.Ratings.Average(r => (double)r.Score) : 0,
            RatingCount   = p.Ratings?.Count ?? 0,
            CreatedAt     = p.CreatedAt,
            UpdatedAt     = p.UpdatedAt
        };

        // ── Supplier Panel Methods ─────────────────────────────────────────────

        public async Task<PagedResult<SupplierProductDto>> GetProductsAsync(
            int companyId, SupplierProductFilter filter, CancellationToken ct = default)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize is < 1 or > 100) filter.PageSize = 10;

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => p.Name.Contains(filter.Search));

            query = filter.SortBy switch
            {
                "price_asc"  => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "stock_asc"  => query.OrderBy(p => p.StockQuantity),
                "name"       => query.OrderBy(p => p.Name),
                _            => query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new SupplierProductDto
                {
                    Id          = p.Id,
                    Name        = p.Name,
                    Category    = p.Company.ProductionType.ToString(),
                    Price       = p.Price,
                    PackageSize = p.PackageSize,
                    StockQty    = p.StockQuantity,
                    StockStatus = p.StockQuantity == 0 ? "Out"
                                : p.StockQuantity < 10  ? "Low"
                                : "In stock",
                    ImageUrl    = p.ImageUrl,
                    IsActive    = p.IsActive,
                    Rating      = p.Ratings.Any() ? p.Ratings.Average(r => (double)r.Score) : 0,
                    ReviewCount = p.Ratings.Count,
                    OrderCount  = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                        .Select(oi => oi.OrderId).Distinct().Count(),
                    Revenue     = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                        .Sum(oi => (decimal?)oi.Quantity * oi.UnitPrice) ?? 0
                })
                .ToListAsync(ct);

            return new PagedResult<SupplierProductDto>
            {
                Items      = items,
                TotalCount = total,
                Page       = filter.Page,
                PageSize   = filter.PageSize
            };
        }

        public async Task<SupplierProductDetailDto?> GetProductDetailAsync(
            int productId, int companyId, CancellationToken ct = default)
        {
            var p = await _context.Products
                .AsNoTracking()
                .Include(x => x.Ratings)
                .Include(x => x.Company)
                .Include(x => x.OrderItems).ThenInclude(oi => oi.Order)
                .FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, ct);

            if (p == null) return null;

            var last30 = DateTime.UtcNow.AddDays(-30);
            var salesChart = p.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Delivered && oi.Order.CreatedAt >= last30)
                .GroupBy(oi => oi.Order.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailySalesDto
                {
                    Date     = g.Key.ToString("yyyy-MM-dd"),
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue  = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .ToList();

            var breakdown = new RatingBreakdownDto
            {
                Five  = p.Ratings.Count(r => r.Score == 5),
                Four  = p.Ratings.Count(r => r.Score == 4),
                Three = p.Ratings.Count(r => r.Score == 3),
                Two   = p.Ratings.Count(r => r.Score == 2),
                One   = p.Ratings.Count(r => r.Score == 1)
            };

            return new SupplierProductDetailDto
            {
                Id          = p.Id,
                Name        = p.Name,
                Description = p.Description,
                Category    = p.Company.ProductionType.ToString(),
                Price       = p.Price,
                PackageSize = p.PackageSize,
                StockQty    = p.StockQuantity,
                StockStatus = p.StockQuantity == 0 ? "Out"
                            : p.StockQuantity < 10  ? "Low"
                            : "In stock",
                ImageUrl    = p.ImageUrl,
                IsActive    = p.IsActive,
                Rating      = p.Ratings.Any() ? p.Ratings.Average(r => (double)r.Score) : 0,
                ReviewCount = p.Ratings.Count,
                OrderCount  = p.OrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                    .Select(oi => oi.OrderId).Distinct().Count(),
                Revenue     = p.OrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                    .Sum(oi => oi.Quantity * oi.UnitPrice),
                SalesChart     = salesChart,
                RatingBreakdown = breakdown
            };
        }

        public async Task<int> CreateProductAsync(
            int companyId, CreateProductRequest request, CancellationToken ct = default)
        {
            var (_, _, created) = await CreateAsync(companyId, request, ct);
            return created?.Id ?? 0;
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(
            int productId, int companyId, UpdateProductRequest request, CancellationToken ct = default)
            => await UpdateAsync(productId, companyId, request, ct);

        public async Task<(bool Success, string Message)> DeleteProductAsync(
            int productId, int companyId, CancellationToken ct = default)
            => await DeleteAsync(productId, companyId, ct);

        public async Task<(bool Success, string Message)> ToggleActiveAsync(
            int productId, int companyId, CancellationToken ct = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki sizga tegishli emas");

            product.IsActive  = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, product.IsActive ? "Mahsulot faollashtirildi" : "Mahsulot o'chirildi");
        }

        public async Task<(bool Success, string Message)> UpdateStockAsync(
            int productId, int companyId, UpdateStockRequest request, Guid changedBy, CancellationToken ct = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, ct);

            if (product == null)
                return (false, "Mahsulot topilmadi yoki sizga tegishli emas");

            if (request.Quantity <= 0)
                return (false, "Miqdor 0 dan katta bo'lishi kerak");

            var changeType = request.ChangeType.ToLowerInvariant() == "set"
                ? StockChangeType.Set
                : StockChangeType.Add;

            var oldQty     = product.StockQuantity;
            product.StockQuantity = changeType == StockChangeType.Set
                ? request.Quantity
                : product.StockQuantity + request.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            if (!Enum.TryParse<StockReason>(request.Reason, true, out var reason))
                reason = StockReason.Correction;

            _context.ProductStockHistories.Add(new ProductStockHistory
            {
                ProductId  = productId,
                ChangeType = changeType,
                Quantity   = request.Quantity,
                Reason     = reason,
                ChangedBy  = changedBy,
                ChangedAt  = DateTime.UtcNow,
                Note       = request.Note
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Stock updated: ProductId={ProductId}, {Old}→{New}, By={UserId}",
                productId, oldQty, product.StockQuantity, changedBy);

            return (true, $"Qoldiq yangilandi: {oldQty} → {product.StockQuantity}");
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(
            int companyId, string period, CancellationToken ct = default)
        {
            var (from, to) = GetPeriodRange(period);

            var data = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.CompanyId == companyId
                          && oi.Order.Status == OrderStatus.Delivered
                          && oi.Order.CreatedAt >= from
                          && oi.Order.CreatedAt < to)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count(),
                    Revenue    = g.Sum(x => (decimal)x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync(ct);

            var totalRevenue = data.Sum(x => x.Revenue);

            return data.Select((x, i) => new TopProductDto
            {
                ProductId  = x.ProductId,
                Name       = x.Name,
                OrderCount = x.OrderCount,
                Revenue    = x.Revenue,
                Percentage = totalRevenue > 0
                    ? Math.Round(x.Revenue / totalRevenue * 100, 1) : 0
            }).ToList();
        }

        private static (DateTime From, DateTime To) GetPeriodRange(string period)
        {
            var now = DateTime.UtcNow;
            return period?.ToLowerInvariant() switch
            {
                "day"   => (now.Date, now.Date.AddDays(1)),
                "month" => (now.AddDays(-30), now),
                "year"  => (now.AddDays(-365), now),
                _       => (now.AddDays(-7), now)   // default: week
            };
        }
    }
}
