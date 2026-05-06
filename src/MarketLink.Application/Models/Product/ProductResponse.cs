namespace MarketLink.Application.Models.Product
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int PackageSize { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
