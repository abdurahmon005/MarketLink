using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Order
{
    public class IncomingOrderFilter
    {
        public OrderStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ShopName { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
