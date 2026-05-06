using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Catalog
{
    public class CatalogProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public CompanyDirection CompanyDirection { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int PackageSize { get; set; }
        public int StockQuantity { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CatalogRatingDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CatalogProductDetailDto : CatalogProductDto
    {
        public string Description { get; set; } = string.Empty;
        public List<CatalogRatingDto> Ratings { get; set; } = new();
    }
}
