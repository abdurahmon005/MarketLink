namespace MarketLink.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        /// <summary>Buyurtma paytidagi narx snapshoti</summary>
        public decimal UnitPrice { get; set; }

        public decimal Subtotal => Quantity * UnitPrice;

        // Navigation
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
