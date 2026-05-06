namespace MarketLink.Application.Models.Cart
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }
}
