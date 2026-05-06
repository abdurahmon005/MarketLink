using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class Shop
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }

        public string FounderName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ShopType ShopType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CertificateUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
