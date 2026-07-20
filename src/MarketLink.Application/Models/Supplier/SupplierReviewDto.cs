namespace MarketLink.Application.Models.Supplier
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? SupplierReply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    public class RatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalCount { get; set; }
        public RatingCountDto Breakdown { get; set; } = new();
    }

    public class RatingCountDto
    {
        public int Five { get; set; }
        public int Four { get; set; }
        public int Three { get; set; }
        public int Two { get; set; }
        public int One { get; set; }
        public double FivePercentage => TotalCount > 0 ? Math.Round((double)Five / TotalCount * 100, 1) : 0;
        public double FourPercentage => TotalCount > 0 ? Math.Round((double)Four / TotalCount * 100, 1) : 0;
        public double ThreePercentage => TotalCount > 0 ? Math.Round((double)Three / TotalCount * 100, 1) : 0;
        public double TwoPercentage => TotalCount > 0 ? Math.Round((double)Two / TotalCount * 100, 1) : 0;
        public double OnePercentage => TotalCount > 0 ? Math.Round((double)One / TotalCount * 100, 1) : 0;
        private int TotalCount => Five + Four + Three + Two + One;
    }

    public class ReviewFilter
    {
        public int? Rating { get; set; }
        public bool? Answered { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReplyRequest
    {
        public string Reply { get; set; } = string.Empty;
    }
}
