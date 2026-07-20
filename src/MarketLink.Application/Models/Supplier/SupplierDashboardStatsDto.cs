namespace MarketLink.Application.Models.Supplier
{
    public class SupplierDashboardStatsDto
    {
        public decimal Revenue { get; set; }
        public decimal RevenueChange { get; set; }
        public int Orders { get; set; }
        public decimal OrdersChange { get; set; }
        public double AvgRating { get; set; }
        public int ActiveShops { get; set; }
        public List<RevenueChartPointDto> ChartData { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<TopBuyerDto> TopBuyers { get; set; } = new();
        public List<ActivityDto> RecentActivity { get; set; } = new();
    }

    public class RevenueChartPointDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopBuyerDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int Rank { get; set; }
    }

    public class ActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RelatedId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
