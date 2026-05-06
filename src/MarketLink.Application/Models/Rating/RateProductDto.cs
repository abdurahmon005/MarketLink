namespace MarketLink.Application.Models.Rating
{
    public class RateProductDto
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        /// <summary>Baho: 1–5</summary>
        public int Score { get; set; }

        public string? Comment { get; set; }
    }

    public class RatingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
