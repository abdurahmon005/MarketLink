using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Shop
{
    public class UpdateShopProfileRequest
    {
        public string? FounderName { get; set; }
        public string? ShopName    { get; set; }
        public string? Address     { get; set; }
        public ShopType? ShopType  { get; set; }
        public string? Description { get; set; }
    }
}
