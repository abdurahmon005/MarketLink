namespace MarketLink.Application.Models.Product
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? PackageSize { get; set; }
        public int? StockQuantity { get; set; }
        public bool? IsActive { get; set; }
    }
}
