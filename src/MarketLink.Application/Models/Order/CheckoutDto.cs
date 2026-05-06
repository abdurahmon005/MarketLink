namespace MarketLink.Application.Models.Order
{
    public class CheckoutDto
    {
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
