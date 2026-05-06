using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Order
{
    public class ShopOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        /// <summary>Bu mahsulotga reyting berilganmi</summary>
        public bool IsRated { get; set; }
    }

    public class ShopOrderDto
    {
        public int OrderId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ShopOrderItemDto> Items { get; set; } = new();
    }

    public class ShopOrderGroupDto
    {
        public DateTime Date { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<ShopOrderDto> Orders { get; set; } = new();
    }

    public class ShopOrderFilterDto
    {
        public OrderStatus? Status { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }
}
