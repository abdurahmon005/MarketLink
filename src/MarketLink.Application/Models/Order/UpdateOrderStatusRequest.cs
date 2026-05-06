using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Order
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}
