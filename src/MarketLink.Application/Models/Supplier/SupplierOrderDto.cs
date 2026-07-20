using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Supplier
{
    public class SupplierOrderListDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupplierOrderDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public string? ShopPhone { get; set; }
        public List<SupplierOrderItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<StatusHistoryDto> StatusHistory { get; set; } = new();
        public DriverInfoDto? DriverInfo { get; set; }
    }

    public class SupplierOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class StatusHistoryDto
    {
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Note { get; set; }
    }

    public class DriverInfoDto
    {
        public Guid? DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int EtaMinutes { get; set; }
    }

    public class SupplierOrderFilter
    {
        public OrderStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AssignDriverRequest
    {
        public Guid DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public string? Notes { get; set; }
    }

    public class RejectOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
