namespace MarketLink.Application.Models.Cart
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
    }

    public class CompanyCartGroup
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal GroupTotal { get; set; }
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public List<CompanyCartGroup> GroupedByCompany { get; set; } = new();
    }
}
