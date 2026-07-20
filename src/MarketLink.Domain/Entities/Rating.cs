namespace MarketLink.Domain.Entities
{
    public class Rating
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public int OrderId { get; set; }

        /// <summary>Baho: 1–5</summary>
        public int Score { get; set; }

        public string? Comment { get; set; }

        public string? SupplierReply { get; set; }
        public DateTime? RepliedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Product Product { get; set; } = null!;
        public Shop Shop { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}
