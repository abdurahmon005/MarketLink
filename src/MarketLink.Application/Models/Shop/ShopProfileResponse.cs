using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Shop
{
    public class ShopProfileResponse
    {
        public int Id { get; set; }
        public string FounderName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ShopType ShopType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CertificateUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
