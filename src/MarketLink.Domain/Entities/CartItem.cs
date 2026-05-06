namespace MarketLink.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Shop Shop { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
