namespace MarketLink.Application.Models.Statistics
{
    public class DailyStatisticsResponse
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public int SoldProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
