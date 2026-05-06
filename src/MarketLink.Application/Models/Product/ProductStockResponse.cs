namespace MarketLink.Application.Models.Product
{
    public class ProductStockResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public double AverageRating { get; set; }
        public bool IsActive { get; set; }
    }
}
