namespace MarketLink.Application.Models.Rating
{
    public class ProductRatingResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductAverageRatingResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int TotalRatings { get; set; }
    }
}
