using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Order
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
