namespace MarketLink.Application.Models.Dashboard
{
    public class DashboardStatsDto
    {
        public MonthStatsDto ThisMonth { get; set; } = new();
        public List<ChartPointDto> Chart { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class MonthStatsDto
    {
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public int DeliveredCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class ChartPointDto
    {
        public DateTime Date { get; set; }
        public decimal Spent { get; set; }
        public int Orders { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public int TotalQty { get; set; }
    }

    public class ActiveDeliveryCardDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int EtaMinutes { get; set; }
        public int ItemCount { get; set; }
    }
}
