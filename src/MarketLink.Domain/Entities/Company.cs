using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string FounderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public CompanyDirection ProductionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CertificateUrl { get; set; }
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
