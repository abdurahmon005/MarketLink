using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class ProductStockHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public StockChangeType ChangeType { get; set; }
        public int Quantity { get; set; }
        public StockReason Reason { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
