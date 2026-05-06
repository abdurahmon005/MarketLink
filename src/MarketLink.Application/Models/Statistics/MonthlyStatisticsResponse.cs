namespace MarketLink.Application.Models.Statistics
{
    public class MonthlyStatisticsResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }

        public string? TopProductName { get; set; }
        public int TopProductSoldCount { get; set; }

        public string? LowestRatedProductName { get; set; }
        public double LowestRatedProductScore { get; set; }
    }
}
