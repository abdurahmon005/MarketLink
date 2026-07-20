namespace MarketLink.Application.Models.Supplier
{
    public class SupplierProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PackageSize { get; set; }
        public int StockQty { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SupplierProductDetailDto : SupplierProductDto
    {
        public string Description { get; set; } = string.Empty;
        public List<DailySalesDto> SalesChart { get; set; } = new();
        public RatingBreakdownDto RatingBreakdown { get; set; } = new();
    }

    public class DailySalesDto
    {
        public string Date { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RatingBreakdownDto
    {
        public int Five { get; set; }
        public int Four { get; set; }
        public int Three { get; set; }
        public int Two { get; set; }
        public int One { get; set; }
        public double FivePercent => Total > 0 ? Math.Round((double)Five / Total * 100, 1) : 0;
        public double FourPercent => Total > 0 ? Math.Round((double)Four / Total * 100, 1) : 0;
        public double ThreePercent => Total > 0 ? Math.Round((double)Three / Total * 100, 1) : 0;
        public double TwoPercent => Total > 0 ? Math.Round((double)Two / Total * 100, 1) : 0;
        public double OnePercent => Total > 0 ? Math.Round((double)One / Total * 100, 1) : 0;
        private int Total => Five + Four + Three + Two + One;
    }

    public class SupplierProductFilter
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UpdateStockRequest
    {
        public string ChangeType { get; set; } = "add";
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
