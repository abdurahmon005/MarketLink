using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        /// <summary>Buyurtma bergan do'kon</summary>
        public int ShopId { get; set; }

        /// <summary>Buyurtma qabul qiluvchi korxona</summary>
        public int CompanyId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal TotalAmount { get; set; }

        public string? Note { get; set; }

        /// <summary>Yetkazish sanasi</summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>Yetkazish manzili</summary>
        public string DeliveryAddress { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Shop Shop { get; set; } = null!;
        public Company Company { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
